using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Text;
using System.Reflection;

namespace C4InterFlow.Automation
{
    public static class NetCodeAnalysisExtensions
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

                return TryGetParentSyntax<T>(syntaxNode, out result);
            }
            catch
            {
                return false;
            }
        }
   
        public static IEnumerable<Document> WithConfirmation(this IEnumerable<Document> documents, string action)
        {
            if (documents.Count() == 0) return documents;

            Console.WriteLine($"The following documents were selected for {action} action:");
            foreach (var document in documents)
            {
                Console.WriteLine(document.FilePath);
            }

            if (GetConfirmation())
            {
                return documents;
            }
            else
            {
                return new List<Document>();
            }
        }

        private static string GetInterfaceAlias(string architectureRootNamespace, string containerAlias, string componentAlias, string interfaceAlias)
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

        public static ITypeSymbol GetReceiverType(this InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel)
        {
            var expression = invocationExpression.Expression;

            if (expression is IdentifierNameSyntax)
            {
                var methodSymbol = semanticModel.GetSymbolInfo(expression).Symbol as IMethodSymbol;
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

            if(invocationExpression.Expression is IdentifierNameSyntax identifierName)
            {
                return identifierName.Identifier.Text;
            }

            return null;
        }

        public static IEnumerable<MethodDeclarationSyntax> WithMethods(this ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>();
        }

        public static IEnumerable<PropertyDeclarationSyntax> WithProperties(this ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>();
        }

        public static IEnumerable<MethodDeclarationSyntax> WithMethods(this InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return interfaceDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>();
        }

        public static ClassDeclarationSyntax AddFlowToComponentInterfaceClass(this ClassDeclarationSyntax classDeclaration,
            NetToNetArchitectureAsCodeWriter writer,
            IEnumerable<NetToNetMethodTriggerMapper>? methodTriggerMappers = null,
            IEnumerable<NetToNetAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            var architectureWorkspace = writer.ArchitectureWorkspace;
            var architectureClassSyntaxTree = classDeclaration.SyntaxTree;
            var architectureClassRoot = architectureClassSyntaxTree.GetRoot();
            var architectureProject = architectureWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == architectureClassSyntaxTree.FilePath));
            var architectureCompilation = architectureProject.GetCompilationAsync().Result;
            var architectureSemanticModel = architectureCompilation.GetSemanticModel(architectureClassSyntaxTree);

            var systemMethodDeclaration = writer.ComponentMethodInterfaceClassMap.GetValueOrDefault(architectureClassSyntaxTree.FilePath);

            if (systemMethodDeclaration == null) return classDeclaration;

            var flowCode = NetToAnyCodeGenerator<NetCodeWriter>.GetFlowCode(
                systemMethodDeclaration,
                new NetArchitectureAsCodeContext(classDeclaration, architectureWorkspace),
                writer,
                alternativeInvocationMappers);

            if (!string.IsNullOrEmpty(flowCode))
            {
                var flowSyntaxNode = architectureClassRoot.DescendantNodes()
                    .OfType<AssignmentExpressionSyntax>()
                    .First(x => x.Left is IdentifierNameSyntax ins && ins.Identifier.Text == "Flow");

                var leadingTrivia = flowSyntaxNode.GetLeadingTrivia();
                if (flowSyntaxNode != null)
                {
                    var newFlowSyntaxNode =
                        SyntaxFactory.ParseExpression($"{leadingTrivia}Flow = {string.Join($"{Environment.NewLine}{leadingTrivia}", flowCode.Split(Environment.NewLine).Where(x => !string.IsNullOrEmpty(x)))}");

                    architectureClassRoot = architectureClassRoot.ReplaceNode(flowSyntaxNode, newFlowSyntaxNode);

                    var document = architectureWorkspace.CurrentSolution.GetDocument(architectureClassSyntaxTree);
                    document.ApplyChanges(architectureClassRoot);
                }
            }

            return classDeclaration;
        }

        public static IEnumerable<MethodDeclarationSyntax> WithConfirmation(this IEnumerable<MethodDeclarationSyntax> methodDeclarations, string action)
        {
            if (methodDeclarations.Count() == 0) return methodDeclarations;

            Console.WriteLine($"The following methods were selected for {action} action:");
            foreach (var methodDeclaration in methodDeclarations)
            {
                Console.WriteLine(methodDeclaration.Identifier.ValueText);
            }

            if (GetConfirmation())
            {
                return methodDeclarations;
            }
            else
            {
                return new List<MethodDeclarationSyntax>();
            }
        }

        public static IEnumerable<InvocationExpressionSyntax> WithInvocations(this MethodDeclarationSyntax methodDeclaration, string[] codeSnippets)
        {
            return methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>()
                .Where(n => codeSnippets.Any(n.ToFullString().Contains));
        }

        public static MethodDeclarationSyntax AddComponentInterfaceClass(this MethodDeclarationSyntax methodDeclaration,
            string softwareSystemName, string containerName, string componentName, NetToNetArchitectureAsCodeWriter writer,
            Func<MethodDeclarationSyntax, SemanticModel, NetToNetArchitectureAsCodeWriter, string?, string?, string?, string>? pathMapper = null,
            string? protocol = null)
        {
            var architectureNamespace = writer.ArchitectureNamespace;
            var architectureProject = writer.ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == architectureNamespace);
            var systemSyntaxTree = methodDeclaration.SyntaxTree;
            var systemProject = writer.SoftwareSystemWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == systemSyntaxTree.FilePath));
            var systemCompilation = systemProject.GetCompilationAsync().Result;
            var systemSemanticModel = systemCompilation.GetSemanticModel(systemSyntaxTree);

            if (architectureProject == null)
            {
                Console.WriteLine($"Project '{architectureNamespace}' was not found in '{writer.ArchitectureWorkspace.CurrentSolution.FilePath}' Solution.");
                return methodDeclaration;
            }

            var interfaceName = methodDeclaration.Identifier.Text;
            var documentName = $"{interfaceName}.cs";

            var projectDirectory = architectureProject.FilePath.Replace($"{architectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, NetToAnyCodeGenerator<NetCodeWriter>.GetComponentInterfacesDirectory(softwareSystemName, containerName, componentName));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (!writer.ComponentMethodInterfaceClassMap.Keys.Contains(filePath))
            {
                writer.ComponentMethodInterfaceClassMap.Add(filePath, methodDeclaration);
            }

            var isPrivate = methodDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PrivateKeyword));
            if(isPrivate)
            {
                var methodSymbol = systemSemanticModel.GetDeclaredSymbol(methodDeclaration) as IMethodSymbol;
                var typeSymbol = methodSymbol.ContainingType;

                if(typeSymbol != null)
                {
                    writer.AddSoftwareSystemTypeMapping(typeSymbol, typeSymbol);
                }
            }

            if (architectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{architectureProject.Name}' Project.");
                return methodDeclaration;
            }

            var interfacePath = string.Empty;

            if(pathMapper != null)
            {
                interfacePath = pathMapper(
                    methodDeclaration, 
                    systemSemanticModel, 
                    writer, 
                    ":", architectureNamespace.Replace(".Architecture", string.Empty), null);
            }

            var sourceCode = NetToAnyCodeGenerator<NetCodeWriter>.GetComponentInterfaceCode(
                architectureNamespace,
                softwareSystemName,
                containerName,
                componentName,
                interfaceName,
                NetCodeWriter.GetLabel(interfaceName),
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

        public static PropertyDeclarationSyntax AddComponentInterfaceClass(this PropertyDeclarationSyntax propertyDeclaration,
            string softwareSystemName, string containerName, string componentName, NetToNetArchitectureAsCodeWriter writer,
            string[] interfaces,
            string? protocol = null)
        {
            var architectureNamespace = writer.ArchitectureNamespace;
            var architectureProject = writer.ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == architectureNamespace);
            var systemSyntaxTree = propertyDeclaration.SyntaxTree;
            var systemProject = writer.SoftwareSystemWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == systemSyntaxTree.FilePath));
            var systemCompilation = systemProject.GetCompilationAsync().Result;
            var systemSemanticModel = systemCompilation.GetSemanticModel(systemSyntaxTree);

            if (architectureProject == null)
            {
                Console.WriteLine($"Project '{architectureNamespace}' was not found in '{writer.ArchitectureWorkspace.CurrentSolution.FilePath}' Solution.");
                return propertyDeclaration;
            }

            foreach(var @interface in interfaces)
            {
                var interfaceName = $"{propertyDeclaration.Identifier.Text}{@interface}";
                var documentName = $"{interfaceName}.cs";

                var projectDirectory = architectureProject.FilePath.Replace($"{architectureProject.Name}.csproj", string.Empty);
                var fileDirectory = Path.Combine(projectDirectory, NetToAnyCodeGenerator<NetCodeWriter>.GetComponentInterfacesDirectory(softwareSystemName, containerName, componentName));
                Directory.CreateDirectory(fileDirectory);

                var filePath = Path.Combine(fileDirectory, documentName);

                if (!writer.ComponentPropertyInterfaceClassMap.Keys.Contains(filePath))
                {
                    writer.ComponentPropertyInterfaceClassMap.Add(filePath, propertyDeclaration);
                }

                if (architectureProject.Documents.Any(x => x.FilePath == filePath))
                {
                    Console.WriteLine($"Document '{filePath}' already exists in '{architectureProject.Name}' Project.");
                    continue;
                }

                var sourceCode = NetToAnyCodeGenerator<NetCodeWriter>.GetComponentInterfaceCode(
                    architectureNamespace,
                    softwareSystemName,
                    containerName,
                    componentName,
                    interfaceName,
                    NetCodeWriter.GetLabel(interfaceName),
                    protocol: protocol);

                var tree = CSharpSyntaxTree.ParseText(sourceCode.ToString());
                var root = tree.GetRoot();
                var formattedRoot = root.NormalizeWhitespace();
                var formattedSourceCode = formattedRoot.ToFullString();

                File.WriteAllText(filePath, formattedSourceCode);
            }

            return propertyDeclaration;
        }
        public static InterfaceDeclarationSyntax AddEntityClass(this InterfaceDeclarationSyntax interfaceDeclaration, string softwareSystemName, string containerName, NetToNetArchitectureAsCodeWriter writer)
        {
            var systemWorkspace = writer.SoftwareSystemWorkspace;
            var systemSyntaxTree = interfaceDeclaration.SyntaxTree;
            var systemProject = systemWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == systemSyntaxTree.FilePath));
            var systemCompilation = systemProject.GetCompilationAsync().Result;
            var systemSemanticModel = systemCompilation.GetSemanticModel(systemSyntaxTree);

            var architectureNamespace = writer.ArchitectureNamespace;
            var architectureProject = writer.ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == architectureNamespace);

            if (architectureProject == null)
            {
                Console.WriteLine($"Project '{architectureNamespace}' was not found in '{writer.ArchitectureWorkspace.CurrentSolution.FilePath}' Solution."); 
                return interfaceDeclaration;
            }

            var entityName = interfaceDeclaration.Identifier.Text;
            var entityTypeSymbol = systemSemanticModel.GetDeclaredSymbol(interfaceDeclaration) as ITypeSymbol;
            var entityAlias = AnyCodeWriter.GetEntityAlias(architectureNamespace, softwareSystemName, containerName, entityName);
            var documentName = $"{entityName}.cs";

            var projectDirectory = architectureProject.FilePath.Replace($"{architectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, NetToAnyCodeGenerator<NetCodeWriter>.GetEntitiesDirectory(softwareSystemName, containerName));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (architectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{architectureProject.Name}' Project.");
                writer.AddEntityTypeMapping(entityTypeSymbol, entityAlias); 
                return interfaceDeclaration;
            }

            var sourceCode = NetToAnyCodeGenerator<NetCodeWriter>.GetEntityCode(
                architectureNamespace,
                softwareSystemName,
                containerName,
                entityName,
                NetCodeWriter.GetLabel(entityName));

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

        public static RecordDeclarationSyntax AddEntityClass(this RecordDeclarationSyntax recordDeclaration, string softwareSystemName, string containerName, NetToNetArchitectureAsCodeWriter writer)
        {
            var systemWorkspace = writer.SoftwareSystemWorkspace;
            var systemSyntaxTree = recordDeclaration.SyntaxTree;
            var systemProject = systemWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == systemSyntaxTree.FilePath));
            var systemCompilation = systemProject.GetCompilationAsync().Result;
            var systemSemanticModel = systemCompilation.GetSemanticModel(systemSyntaxTree);

            var architectureNamespace = writer.ArchitectureNamespace;
            var architectureProject = writer.ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == architectureNamespace);

            if (architectureProject == null)
            {
                Console.WriteLine($"Project '{architectureNamespace}' was not found in '{writer.ArchitectureWorkspace.CurrentSolution.FilePath}' Solution.");
                return recordDeclaration;
            }

            var entityName = recordDeclaration.Identifier.Text;
            var entityTypeSymbol = systemSemanticModel.GetDeclaredSymbol(recordDeclaration) as ITypeSymbol;
            var entityAlias = AnyCodeWriter.GetEntityAlias(architectureNamespace,softwareSystemName,containerName, entityName);

            var documentName = $"{entityName}.cs";

            var projectDirectory = architectureProject.FilePath.Replace($"{architectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, NetToAnyCodeGenerator<NetCodeWriter>.GetEntitiesDirectory(softwareSystemName, containerName));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (architectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{architectureProject.Name}' Project.");
                writer.AddEntityTypeMapping(entityTypeSymbol, entityAlias);
                return recordDeclaration;
            }

            var sourceCode = NetToAnyCodeGenerator<NetCodeWriter>.GetEntityCode(
                architectureNamespace,
                softwareSystemName,
                containerName,
                entityName,
                NetCodeWriter.GetLabel(entityName));

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
            var result = string.Empty;

            var aliasField = classDeclaration.Members
                .OfType<FieldDeclarationSyntax>()
                .SelectMany(f => f.Declaration.Variables)
                .FirstOrDefault(v => v.Identifier.Text == "ALIAS");

            if (aliasField != null)
            {
                var aliasInitializer = aliasField.Initializer;
                if (aliasInitializer != null)
                {
                    result = aliasInitializer.Value.ToString().Replace("\"", string.Empty);
                }
            }

            return result;
        }
        public static ClassDeclarationSyntax AddComponentClass(this ClassDeclarationSyntax classDeclaration, string softwareSystemName, string containerName, NetToNetArchitectureAsCodeWriter writer)
        {
            var architectureNamespace = writer.ArchitectureNamespace;
            var project = writer.ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == architectureNamespace);
            
            if (project == null)
            {
                Console.WriteLine($"Project '{architectureNamespace}' was not found in '{writer.ArchitectureWorkspace.CurrentSolution.FilePath}' Solution.");
                return classDeclaration;
            }
            
            var componentName = classDeclaration.Identifier.Text;
            var documentName = $"{componentName}.cs";

            var projectDirectory = project.FilePath.Replace($"{project.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, NetToAnyCodeGenerator<NetCodeWriter>.GetComponentsDirectory(softwareSystemName, containerName)); ;
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (project.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{project.Name}' Project.");
                return classDeclaration;
            }

            var sourceCode = NetToAnyCodeGenerator<NetCodeWriter>.GetComponentCode(
                architectureNamespace,
                softwareSystemName,
                containerName,
                componentName,
                NetCodeWriter.GetLabel(componentName));

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

        public static InterfaceDeclarationSyntax AddComponentClass(this InterfaceDeclarationSyntax interfaceDeclaration, string softwareSystemName, string containerName, NetToNetArchitectureAsCodeWriter writer)
        {
            var architectureNamespace = writer.ArchitectureNamespace;
            var project = writer.ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == architectureNamespace);

            if (project == null)
            {
                Console.WriteLine($"Project '{architectureNamespace}' was not found in '{writer.ArchitectureWorkspace.CurrentSolution.FilePath}' Solution.");
                return interfaceDeclaration;
            }

            var componentName = interfaceDeclaration.Identifier.Text;
            var documentName = $"{componentName}.cs";

            var projectDirectory = project.FilePath.Replace($"{project.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, NetToAnyCodeGenerator<NetCodeWriter>.GetComponentsDirectory(softwareSystemName, containerName));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (project.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{project.Name}' Project.");
                return interfaceDeclaration;
            }

            var sourceCode = NetToAnyCodeGenerator<NetCodeWriter>.GetComponentCode(
                architectureNamespace,
                softwareSystemName,
                containerName,
                componentName,
                NetCodeWriter.GetLabel(componentName));

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

        public static IEnumerable<ClassDeclarationSyntax> WithConfirmation(this IEnumerable<ClassDeclarationSyntax> classDeclarations, string action)
        {
            if (classDeclarations.Count() == 0) return classDeclarations;

            Console.WriteLine($"The following classes were selected for {action} action:");
            foreach (var classDeclaration in classDeclarations)
            {
                Console.WriteLine(classDeclaration.Identifier.ValueText);
            }

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

        public static IEnumerable<ITypeSymbol> GetGenericInvocationTypes(this InvocationExpressionSyntax invocationExpression, Project project)
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

        public static IEnumerable<ITypeSymbol> GetGenericInvocationTypes(this InvocationExpressionSyntax invocationExpression, Document document)
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
