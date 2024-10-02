﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Reflection;
using C4InterFlow.Automation.Readers;
using Serilog;
using System.Text;

namespace C4InterFlow.Automation.Writers
{
    public static class CSharpToCSharpExtensions
    {
        private static bool GetConfirmation()
        {
            Console.WriteLine("Would you like to proceed (Y/N):");
            string input = Console.ReadLine();

            while (input != "Y" && input != "N")
            {
                Console.WriteLine("Invalid input. Enter Y or N:");
                input = Console.ReadLine();
            }

            return input == "Y" ? true : false;
        }

        public static bool TryGetParentSyntax<T>(this SyntaxNode syntaxNode, out T result) where T : SyntaxNode
        {
            result = null;

            if (syntaxNode == null)
            {
                return false;
            }

            try
            {
                syntaxNode = syntaxNode.Parent;

                if (syntaxNode == null)
                {
                    return false;
                }

                if (syntaxNode.GetType() == typeof(T))
                {
                    result = syntaxNode as T;
                    return true;
                }

                return TryGetParentSyntax(syntaxNode, out result);
            }
            catch
            {
                return false;
            }
        }

        public static IEnumerable<Document> WithConfirmation(this IEnumerable<Document> documents, string action)
        {
            if (documents.Count() == 0) return documents;

            Log.Information("The following {@Documents} were selected for {Action}", documents, action);
            // Console.WriteLine($"The following documents were selected for {action} action:");
            // foreach (var document in documents)
            // {
            //     Console.WriteLine(document.FilePath);
            // }

            if (GetConfirmation())
            {
                return documents;
            }
            else
            {
                return new List<Document>();
            }
        }

        private static string GetInterfaceAlias(string architectureRootNamespace, string containerAlias,
            string componentAlias, string interfaceAlias)
        {
            containerAlias = containerAlias.Substring(containerAlias.IndexOf(".Containers."));
            componentAlias = componentAlias.Substring(componentAlias.IndexOf(".Components."));

            return $"{architectureRootNamespace}{containerAlias}{componentAlias}.Interfaces.{interfaceAlias}.ALIAS";
        }

        private static SyntaxNode AddUsingDirective(SyntaxNode documentRoot, string namespaceToAdd)
        {
            var compilationUnit = documentRoot as CompilationUnitSyntax;

            if (compilationUnit == null)
            {
                return documentRoot;
            }

            if (!compilationUnit.Usings.Any(u => u.Name.ToString() == namespaceToAdd))
            {
                var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($" {namespaceToAdd}"))
                    .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
                var newUsings = compilationUnit.Usings.Add(usingDirective);
                var newCompilationUnit = compilationUnit.WithUsings(newUsings);

                return documentRoot.ReplaceNode(compilationUnit, newCompilationUnit);
            }

            return documentRoot;
        }

        public static void ApplyChanges(this Document document, SyntaxNode root)
        {
            var source = document.WithSyntaxRoot(root).GetTextAsync().Result;
            using (var writer = new StreamWriter(document.FilePath, append: false, encoding: source.Encoding))
            {
                source.Write(writer);
            }
        }

        public static ITypeSymbol GetReceiverType(this InvocationExpressionSyntax invocationExpression,
            SemanticModel semanticModel)
        {
            var expression = invocationExpression.Expression;

            if (expression is IdentifierNameSyntax)
            {
                var methodSymbol = (semanticModel.GetSymbolInfo(expression).Symbol ??
                                    (semanticModel.GetSymbolInfo(expression).CandidateSymbols != null
                                        ? semanticModel.GetSymbolInfo(expression).CandidateSymbols.FirstOrDefault()
                                        : default)
                    ) as IMethodSymbol;
                return methodSymbol?.ReceiverType;
            }

            var receiver = invocationExpression.Expression;

            while (receiver is MemberAccessExpressionSyntax memberAccess)
            {
                receiver = memberAccess.Expression;
            }

            return semanticModel.GetTypeInfo(receiver).Type;
        }

        public static string GetInvokedMemberName(this InvocationExpressionSyntax invocationExpression)
        {
            if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.Name.Identifier.Text;
            }

            if (invocationExpression.Expression is IdentifierNameSyntax identifierName)
            {
                return identifierName.Identifier.Text;
            }

            return null;
        }

        public static IEnumerable<MethodDeclarationSyntax> WithMethods(this ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>();
        }

        public static IEnumerable<PropertyDeclarationSyntax> WithProperties(
            this ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>();
        }

        public static IEnumerable<MethodDeclarationSyntax> WithMethods(
            this InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return interfaceDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>();
        }

        public static ClassDeclarationSyntax AddFlowToComponentInterfaceClass(
            this ClassDeclarationSyntax classDeclaration,
            CSharpToCSharpAaCWriter writer,
            IEnumerable<CSharpToAnyMethodTriggerMapper>? methodTriggerMappers = null,
            IEnumerable<NetToAnyAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            var architectureWorkspace = writer.ArchitectureWorkspace;
            var architectureClassSyntaxTree = classDeclaration.SyntaxTree;
            var architectureClassRoot = architectureClassSyntaxTree.GetRoot();
            var architectureProject = architectureWorkspace.CurrentSolution.Projects.FirstOrDefault(p =>
                p.Documents.Any(d => d.FilePath == architectureClassSyntaxTree.FilePath));
            var architectureCompilation = architectureProject.GetCompilationAsync().Result;
            var architectureSemanticModel = architectureCompilation.GetSemanticModel(architectureClassSyntaxTree);

            var systemMethodDeclaration =
                writer.ComponentInterfaceAaCFileToCSharpMethodDeclarationMap.GetValueOrDefault(
                    architectureClassSyntaxTree.FilePath);

            if (systemMethodDeclaration == null) return classDeclaration;

            var flowCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetFlowCode(
                systemMethodDeclaration,
                new CSharpAaCReaderStrategy(classDeclaration, architectureWorkspace),
                writer,
                alternativeInvocationMappers);

            if (!string.IsNullOrEmpty(flowCode))
            {
                var flowSyntaxNode = architectureClassRoot.DescendantNodes()
                    .OfType<AssignmentExpressionSyntax>()
                    .First(x => x.Left is IdentifierNameSyntax ins && ins.Identifier.Text == "Flow");

                if (flowSyntaxNode != null)
                {
                    var leadingTrivia = flowSyntaxNode.GetLeadingTrivia();

                    // Get the right part of the existing assignment (Right-hand side of Flow = ...)
                    var rightExpression = flowSyntaxNode.Right;

                    // Parse the new flowCode and append it to the existing right expression
                    var newFlowSyntaxNode = SyntaxFactory.ParseExpression(
                        $"{rightExpression}{Environment.NewLine}{leadingTrivia}{string.Join($"{Environment.NewLine}{leadingTrivia}", flowCode.Split(Environment.NewLine).Where(x => !string.IsNullOrEmpty(x)))}"
                    );

                    // Replace the existing right-hand side with the new combined expression
                    var updatedFlowSyntaxNode = flowSyntaxNode.WithRight(newFlowSyntaxNode);

                    architectureClassRoot = architectureClassRoot.ReplaceNode(flowSyntaxNode, updatedFlowSyntaxNode);

                    var document = architectureWorkspace.CurrentSolution.GetDocument(architectureClassSyntaxTree);
                    document.ApplyChanges(architectureClassRoot);
                }
            }

            return classDeclaration;
        }


        public static IEnumerable<MethodDeclarationSyntax> WithConfirmation(
            this IEnumerable<MethodDeclarationSyntax> methodDeclarations, string action)
        {
            if (methodDeclarations.Count() == 0) return methodDeclarations;

            
            Log.Information("The following methods were selected for {Action} action: {Methods}", action, methodDeclarations.Select(x=>x.Identifier.ValueText));
            // Console.WriteLine($"The following methods were selected for {action} action:");
            // foreach (var methodDeclaration in methodDeclarations)
            // {
            //     Console.WriteLine(methodDeclaration.Identifier.ValueText);
            // }

            if (GetConfirmation())
            {
                return methodDeclarations;
            }
            else
            {
                return new List<MethodDeclarationSyntax>();
            }
        }

        public static IEnumerable<InvocationExpressionSyntax> WithInvocations(
            this MethodDeclarationSyntax methodDeclaration, string[] codeSnippets)
        {
            return methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>()
                .Where(n => codeSnippets.Any(n.ToFullString().Contains));
        }

        public static MethodDeclarationSyntax AddComponentInterfaceClass(this MethodDeclarationSyntax methodDeclaration,
            string softwareSystemName, string containerName, string componentName, CSharpToCSharpAaCWriter writer,
            Func<MethodDeclarationSyntax, SemanticModel, CSharpToCSharpAaCWriter, string?, string?, string?, string>?
                pathMapper = null,
            string? protocol = null)
        {
            var architectureNamespace = writer.ArchitectureNamespace;
            var architectureProject =
                writer.ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x =>
                    x.Name == architectureNamespace);
            var systemSyntaxTree = methodDeclaration.SyntaxTree;
            var systemProject =
                writer.SoftwareSystemWorkspace.CurrentSolution.Projects.FirstOrDefault(p =>
                    p.Documents.Any(d => d.FilePath == systemSyntaxTree.FilePath));
            var systemCompilation = systemProject.GetCompilationAsync().Result;
            var systemSemanticModel = systemCompilation.GetSemanticModel(systemSyntaxTree);

            if (architectureProject == null)
            {
                Log.Warning("Project {Name} was not found in {Solution} solution", architectureNamespace, writer.ArchitectureWorkspace.CurrentSolution.FilePath);
                
                return methodDeclaration;
            }

            var interfaceName = methodDeclaration.Identifier.Text;
            var documentName = $"{interfaceName}.cs";

            var projectDirectory =
                architectureProject.FilePath.Replace($"{architectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory,
                CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetComponentInterfacesDirectory(softwareSystemName,
                    containerName, componentName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (!writer.ComponentInterfaceAaCFileToCSharpMethodDeclarationMap.Keys.Contains(filePath))
            {
                writer.ComponentInterfaceAaCFileToCSharpMethodDeclarationMap.Add(filePath, methodDeclaration);
            }

            var isPrivate = methodDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PrivateKeyword));
            if (isPrivate)
            {
                var methodSymbol = systemSemanticModel.GetDeclaredSymbol(methodDeclaration) as IMethodSymbol;
                var typeSymbol = methodSymbol.ContainingType;

                if (typeSymbol != null)
                {
                    writer.AddSoftwareSystemTypeMapping(typeSymbol, typeSymbol);
                }
            }

            if (architectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Log.Warning("Document {Name} already exists in {Project} project", filePath, architectureProject.Name);

                return methodDeclaration;
            }

            var interfacePath = string.Empty;

            if (pathMapper != null)
            {
                interfacePath = pathMapper(
                    methodDeclaration,
                    systemSemanticModel,
                    writer,
                    ":", architectureNamespace.Replace(".Architecture", string.Empty), null);
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetComponentInterfaceCode(
                architectureNamespace,
                softwareSystemName,
                containerName,
                componentName,
                interfaceName,
                CSharpCodeWriter.GetLabel(interfaceName),
                protocol: protocol,
                path: !string.IsNullOrEmpty(interfacePath) ? interfacePath : string.Empty,
                isPrivate: isPrivate);

            var tree = CSharpSyntaxTree.ParseText(sourceCode.ToString());
            var root = tree.GetRoot();
            var formattedRoot = root.NormalizeWhitespace();
            var formattedSourceCode = formattedRoot.ToFullString();

            //TODO: Add support for Interface method overloads
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, formattedSourceCode);
            }

            return methodDeclaration;
        }

        public static PropertyDeclarationSyntax AddComponentInterfaceClass(
            this PropertyDeclarationSyntax propertyDeclaration,
            string softwareSystemName, string containerName, string componentName, CSharpToCSharpAaCWriter writer,
            string[] interfaces,
            string? protocol = null)
        {
            var architectureNamespace = writer.ArchitectureNamespace;
            var architectureProject =
                writer.ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x =>
                    x.Name == architectureNamespace);
            var systemSyntaxTree = propertyDeclaration.SyntaxTree;
            var systemProject =
                writer.SoftwareSystemWorkspace.CurrentSolution.Projects.FirstOrDefault(p =>
                    p.Documents.Any(d => d.FilePath == systemSyntaxTree.FilePath));
            var systemCompilation = systemProject.GetCompilationAsync().Result;
            var systemSemanticModel = systemCompilation.GetSemanticModel(systemSyntaxTree);

            if (architectureProject == null)
            {
                Log.Warning( "Project {Name} was not found in {Solution} solution", architectureNamespace, writer.ArchitectureWorkspace.CurrentSolution.FilePath);

                return propertyDeclaration;
            }

            foreach (var @interface in interfaces)
            {
                var interfaceName = $"{propertyDeclaration.Identifier.Text}{@interface}";
                var documentName = $"{interfaceName}.cs";

                var projectDirectory =
                    architectureProject.FilePath.Replace($"{architectureProject.Name}.csproj", string.Empty);
                var fileDirectory = Path.Combine(projectDirectory,
                    CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetComponentInterfacesDirectory(softwareSystemName,
                        containerName, componentName));
                var filePath = Path.Combine(fileDirectory, documentName);

                Directory.CreateDirectory(fileDirectory);

                if (!writer.ComponentInterfaceAaCFileToCSharpPropertyDeclarationMap.Keys.Contains(filePath))
                {
                    writer.ComponentInterfaceAaCFileToCSharpPropertyDeclarationMap.Add(filePath, propertyDeclaration);
                }

                if (architectureProject.Documents.Any(x => x.FilePath == filePath))
                {
                    Log.Warning("Document {Name} already exists in {Project} project", filePath, architectureProject.Name);

                    continue;
                }

                var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetComponentInterfaceCode(
                    architectureNamespace,
                    softwareSystemName,
                    containerName,
                    componentName,
                    interfaceName,
                    CSharpCodeWriter.GetLabel(interfaceName),
                    protocol: protocol);

                var tree = CSharpSyntaxTree.ParseText(sourceCode.ToString());
                var root = tree.GetRoot();
                var formattedRoot = root.NormalizeWhitespace();
                var formattedSourceCode = formattedRoot.ToFullString();

                File.WriteAllText(filePath, formattedSourceCode);
            }

            return propertyDeclaration;
        }

        public static InterfaceDeclarationSyntax AddEntityClass(this InterfaceDeclarationSyntax interfaceDeclaration,
            string softwareSystemName, string containerName, CSharpToCSharpAaCWriter writer)
        {
            var systemWorkspace = writer.SoftwareSystemWorkspace;
            var systemSyntaxTree = interfaceDeclaration.SyntaxTree;
            var systemProject = systemWorkspace.CurrentSolution.Projects.FirstOrDefault(p =>
                p.Documents.Any(d => d.FilePath == systemSyntaxTree.FilePath));
            var systemCompilation = systemProject.GetCompilationAsync().Result;
            var systemSemanticModel = systemCompilation.GetSemanticModel(systemSyntaxTree);

            var architectureNamespace = writer.ArchitectureNamespace;
            var architectureProject =
                writer.ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x =>
                    x.Name == architectureNamespace);

            if (architectureProject == null)
            {
                Log.Warning("Project {Name} was not found in {Solution} solution", architectureNamespace, writer.ArchitectureWorkspace.CurrentSolution.FilePath);

                return interfaceDeclaration;
            }

            var entityName = interfaceDeclaration.Identifier.Text;
            var entityTypeSymbol = systemSemanticModel.GetDeclaredSymbol(interfaceDeclaration) as ITypeSymbol;
            var entityAlias =
                AnyCodeWriter.GetEntityAlias(architectureNamespace, softwareSystemName, containerName, entityName);
            var documentName = $"{entityName}.cs";

            var projectDirectory =
                architectureProject.FilePath.Replace($"{architectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory,
                CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetEntitiesDirectory(softwareSystemName, containerName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (architectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Log.Warning("Document {Name} already exists in {Project} project", filePath, architectureProject.Name);
                
                writer.AddEntityTypeMapping(entityTypeSymbol, entityAlias);
                return interfaceDeclaration;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetEntityCode(
                architectureNamespace,
                softwareSystemName,
                containerName,
                entityName,
                CSharpCodeWriter.GetLabel(entityName));

            var tree = CSharpSyntaxTree.ParseText(sourceCode.ToString());
            var root = tree.GetRoot();
            var formattedRoot = root.NormalizeWhitespace();
            var formattedSourceCode = formattedRoot.ToFullString();

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, formattedSourceCode);
            }

            writer.AddEntityTypeMapping(entityTypeSymbol, entityAlias);
            return interfaceDeclaration;
        }

        public static RecordDeclarationSyntax AddEntityClass(this RecordDeclarationSyntax recordDeclaration,
            string softwareSystemName, string containerName, CSharpToCSharpAaCWriter writer)
        {
            var systemWorkspace = writer.SoftwareSystemWorkspace;
            var systemSyntaxTree = recordDeclaration.SyntaxTree;
            var systemProject = systemWorkspace.CurrentSolution.Projects.FirstOrDefault(p =>
                p.Documents.Any(d => d.FilePath == systemSyntaxTree.FilePath));
            var systemCompilation = systemProject.GetCompilationAsync().Result;
            var systemSemanticModel = systemCompilation.GetSemanticModel(systemSyntaxTree);

            var architectureNamespace = writer.ArchitectureNamespace;
            var architectureProject =
                writer.ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x =>
                    x.Name == architectureNamespace);

            if (architectureProject == null)
            {
                Log.Warning( "Project {Name} was not found in {Solution} solution", architectureNamespace, writer.ArchitectureWorkspace.CurrentSolution.FilePath);

                return recordDeclaration;
            }

            var entityName = recordDeclaration.Identifier.Text;
            var entityTypeSymbol = systemSemanticModel.GetDeclaredSymbol(recordDeclaration) as ITypeSymbol;
            var entityAlias =
                AnyCodeWriter.GetEntityAlias(architectureNamespace, softwareSystemName, containerName, entityName);

            var documentName = $"{entityName}.cs";

            var projectDirectory =
                architectureProject.FilePath.Replace($"{architectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory,
                CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetEntitiesDirectory(softwareSystemName, containerName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (architectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Log.Warning("Document {Name} already exists in {Project} project", filePath, architectureProject.Name);

                writer.AddEntityTypeMapping(entityTypeSymbol, entityAlias);
                return recordDeclaration;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetEntityCode(
                architectureNamespace,
                softwareSystemName,
                containerName,
                entityName,
                CSharpCodeWriter.GetLabel(entityName));

            var tree = CSharpSyntaxTree.ParseText(sourceCode.ToString());
            var root = tree.GetRoot();
            var formattedRoot = root.NormalizeWhitespace();
            var formattedSourceCode = formattedRoot.ToFullString();

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, formattedSourceCode);
            }

            writer.AddEntityTypeMapping(entityTypeSymbol, entityAlias);
            return recordDeclaration;
        }

        public static string GetAliasFieldValue(this ClassDeclarationSyntax classDeclaration)
        {
            var result = new StringBuilder();

            // Traverse up the syntax tree to get the namespace declaration
            var namespaceDeclaration = classDeclaration.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            if (namespaceDeclaration != null)
            {
                result.Append(namespaceDeclaration.Name.ToString());
            }

            // Traverse up the syntax tree to gather all parent class declarations for nested classes
            var currentClass = classDeclaration;
            var currentClassPathSegments = new List<string>();
            while (currentClass != null)
            {
                currentClassPathSegments.Add(currentClass.Identifier.Text);
                currentClass = currentClass.Parent as ClassDeclarationSyntax;
            }

            currentClassPathSegments.Reverse();
            result.Append($".{string.Join('.', currentClassPathSegments.ToArray())}");

            return result.ToString();
        }

        public static ClassDeclarationSyntax AddComponentClass(this ClassDeclarationSyntax classDeclaration,
            string softwareSystemName, string containerName, CSharpToCSharpAaCWriter writer)
        {
            var architectureNamespace = writer.ArchitectureNamespace;
            var project =
                writer.ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x =>
                    x.Name == architectureNamespace);

            if (project == null)
            {
                Log.Warning( "Project {Name} was not found in {Solution} solution", architectureNamespace, writer.ArchitectureWorkspace.CurrentSolution.FilePath);

                return classDeclaration;
            }

            var componentName = classDeclaration.Identifier.Text;
            var documentName = $"{componentName}.cs";

            var projectDirectory = project.FilePath.Replace($"{project.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory,
                CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetComponentsDirectory(softwareSystemName, containerName));
            ;
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (project.Documents.Any(x => x.FilePath == filePath))
            {
                Log.Warning("Document {Name} already exists in {Project} project", filePath, project.Name);

                return classDeclaration;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetComponentCode(
                architectureNamespace,
                softwareSystemName,
                containerName,
                componentName,
                CSharpCodeWriter.GetLabel(componentName));

            var tree = CSharpSyntaxTree.ParseText(sourceCode.ToString());
            var root = tree.GetRoot();
            var formattedRoot = root.NormalizeWhitespace();
            var formattedSourceCode = formattedRoot.ToFullString();

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, formattedSourceCode);
            }

            return classDeclaration;
        }

        public static InterfaceDeclarationSyntax AddComponentClass(this InterfaceDeclarationSyntax interfaceDeclaration,
            string softwareSystemName, string containerName, CSharpToCSharpAaCWriter writer)
        {
            var architectureNamespace = writer.ArchitectureNamespace;
            var project =
                writer.ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x =>
                    x.Name == architectureNamespace);

            if (project == null)
            {
                Log.Warning( "Project {Name} was not found in {Solution} solution", architectureNamespace, writer.ArchitectureWorkspace.CurrentSolution.FilePath);

                return interfaceDeclaration;
            }

            var componentName = interfaceDeclaration.Identifier.Text;
            var documentName = $"{componentName}.cs";

            var projectDirectory = project.FilePath.Replace($"{project.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory,
                CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetComponentsDirectory(softwareSystemName, containerName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (project.Documents.Any(x => x.FilePath == filePath))
            {
                Log.Warning("Document {Name} already exists in {Project} project", filePath, project.Name);

                return interfaceDeclaration;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetComponentCode(
                architectureNamespace,
                softwareSystemName,
                containerName,
                componentName,
                CSharpCodeWriter.GetLabel(componentName));

            var tree = CSharpSyntaxTree.ParseText(sourceCode.ToString());
            var root = tree.GetRoot();
            var formattedRoot = root.NormalizeWhitespace();
            var formattedSourceCode = formattedRoot.ToFullString();

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, formattedSourceCode);
            }

            return interfaceDeclaration;
        }

        public static IEnumerable<ClassDeclarationSyntax> WithConfirmation(
            this IEnumerable<ClassDeclarationSyntax> classDeclarations, string action)
        {
            if (classDeclarations.Count() == 0) return classDeclarations;

            Log.Information("The following classes {Classes} were selected for {Action} action", classDeclarations.Select(x=>x.Identifier.ValueText), action);
            //Console.WriteLine($"The following classes were selected for {action} action:");
            // foreach (var classDeclaration in classDeclarations)
            // {
            //     Console.WriteLine(classDeclaration.Identifier.ValueText);
            // }

            if (GetConfirmation())
            {
                return classDeclarations;
            }
            else
            {
                return new List<ClassDeclarationSyntax>();
            }
        }

        public static ClassDeclarationSyntax GetTypeDefinition(this Workspace workspace, string typeName)
        {
            foreach (var project in workspace.CurrentSolution.Projects)
            {
                var compilation = project.GetCompilationAsync().Result;

                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    var semanticModel = compilation.GetSemanticModel(syntaxTree);

                    var root = syntaxTree.GetRoot();

                    var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

                    foreach (var classDeclaration in classDeclarations)
                    {
                        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
                        if (classSymbol.ToDisplayString().Equals(typeName))
                        {
                            return classDeclaration;
                        }
                    }
                }
            }

            return null;
        }

        public static IEnumerable<ClassDeclarationSyntax> WithClasses(this Document document)
        {
            var syntaxTree = document.GetSyntaxTreeAsync().Result;
            var root = syntaxTree.GetRoot();

            return root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        }

        public static IEnumerable<InterfaceDeclarationSyntax> WithInterfaces(this Document document)
        {
            var syntaxTree = document.GetSyntaxTreeAsync().Result;
            var root = syntaxTree.GetRoot();

            return root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
        }

        public static IEnumerable<RecordDeclarationSyntax> WithRecords(this Document document)
        {
            var syntaxTree = document.GetSyntaxTreeAsync().Result;
            var root = syntaxTree.GetRoot();

            return root.DescendantNodes().OfType<RecordDeclarationSyntax>();
        }

        public static IEnumerable<ITypeSymbol> GetGenericInvocationTypes(
            this InvocationExpressionSyntax invocationExpression, Project project)
        {
            var result = new List<ITypeSymbol>();

            var compilation = project.GetCompilationAsync().Result;
            var semanticModel = compilation.GetSemanticModel(invocationExpression.SyntaxTree);
            if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Name is GenericNameSyntax genericName)
                {
                    var arguments = genericName.TypeArgumentList?.Arguments;

                    if (arguments != null)
                    {
                        foreach (var argument in arguments)
                        {
                            result.Add(semanticModel.GetTypeInfo(argument).Type);
                        }
                    }
                }
            }

            return result;
        }

        public static IEnumerable<ITypeSymbol> GetGenericInvocationTypes(
            this InvocationExpressionSyntax invocationExpression, Document document)
        {
            var result = new List<ITypeSymbol>();

            var semanticModel = document.GetSemanticModelAsync().Result;

            if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Name is GenericNameSyntax genericName)
                {
                    var arguments = genericName.TypeArgumentList?.Arguments;

                    if (arguments != null)
                    {
                        foreach (var argument in arguments)
                        {
                            result.Add(semanticModel.GetTypeInfo(argument).Type);
                        }
                    }
                }
            }

            return result;
        }
    }
}