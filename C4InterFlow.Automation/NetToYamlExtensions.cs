using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Text;
using System.Reflection;

namespace C4InterFlow.Automation
{
    public static class NetToYamlExtensions
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
        /*
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
        */
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

        /*
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
        */

        public static MethodDeclarationSyntax AddComponentInterfaceYamlFile(this MethodDeclarationSyntax methodDeclaration,
            string softwareSystemName, string containerName, string componentName, NetToYamlArchitectureAsCodeWriter writer,
            Func<MethodDeclarationSyntax, SemanticModel, NetToYamlArchitectureAsCodeWriter, string?, string?, string?, string>? pathMapper = null,
            string? protocol = null)
        {
            var architectureNamespace = writer.ArchitectureNamespace;
            var systemSyntaxTree = methodDeclaration.SyntaxTree;
            var systemProject = writer.SoftwareSystemWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == systemSyntaxTree.FilePath));
            var systemCompilation = systemProject.GetCompilationAsync().Result;
            var systemSemanticModel = systemCompilation.GetSemanticModel(systemSyntaxTree);

            var interfaceName = methodDeclaration.Identifier.Text;
            var documentName = $"{interfaceName}.yaml";

            var fileDirectory = Path.Combine(writer.ArchitectureOutputPath, NetToAnyCodeGenerator<YamlCodeWriter>.GetComponentInterfacesDirectory(softwareSystemName, containerName, componentName));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (!writer.ComponentMethodInterfaceObjectMap.Keys.Contains(filePath))
            {
                writer.ComponentMethodInterfaceObjectMap.Add(filePath, methodDeclaration);
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

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
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

            var sourceCode = NetToAnyCodeGenerator<YamlCodeWriter>.GetComponentInterfaceCode(
                architectureNamespace,
                softwareSystemName,
                containerName,
                componentName,
                interfaceName,
                YamlCodeWriter.GetLabel(interfaceName),
                protocol: protocol,
                path: !string.IsNullOrEmpty(interfacePath) ? interfacePath : string.Empty,
                isPrivate: isPrivate);

            //TODO: Add support for Interface method overloads
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, sourceCode);
            }

            return methodDeclaration;
        }

        public static PropertyDeclarationSyntax AddComponentInterfaceYamlFile(this PropertyDeclarationSyntax propertyDeclaration,
            string softwareSystemName, string containerName, string componentName, NetToYamlArchitectureAsCodeWriter writer,
            string[] interfaces,
            string? protocol = null)
        {
            var architectureNamespace = writer.ArchitectureNamespace;
            
            var systemSyntaxTree = propertyDeclaration.SyntaxTree;
            var systemProject = writer.SoftwareSystemWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == systemSyntaxTree.FilePath));
            var systemCompilation = systemProject.GetCompilationAsync().Result;
            var systemSemanticModel = systemCompilation.GetSemanticModel(systemSyntaxTree);

            foreach(var @interface in interfaces)
            {
                var interfaceName = $"{propertyDeclaration.Identifier.Text}{@interface}";
                var documentName = $"{interfaceName}.yaml";

                var fileDirectory = Path.Combine(writer.ArchitectureOutputPath, NetToAnyCodeGenerator<YamlCodeWriter>.GetComponentInterfacesDirectory(softwareSystemName, containerName, componentName));
                Directory.CreateDirectory(fileDirectory);

                var filePath = Path.Combine(fileDirectory, documentName);

                if (!writer.ComponentPropertyInterfaceObjectMap.Keys.Contains(filePath))
                {
                    writer.ComponentPropertyInterfaceObjectMap.Add(filePath, propertyDeclaration);
                }

                if (File.Exists(filePath))
                {
                    Console.WriteLine($"Document '{filePath}' already exists.");
                    continue;
                }

                var sourceCode = NetToAnyCodeGenerator<YamlCodeWriter>.GetComponentInterfaceCode(
                    architectureNamespace,
                    softwareSystemName,
                    containerName,
                    componentName,
                    interfaceName,
                    YamlCodeWriter.GetLabel(interfaceName),
                    protocol: protocol);

                File.WriteAllText(filePath, sourceCode);
            }

            return propertyDeclaration;
        }
        public static InterfaceDeclarationSyntax AddEntityYamlFile(this InterfaceDeclarationSyntax interfaceDeclaration, string softwareSystemName, string containerName, NetToYamlArchitectureAsCodeWriter writer)
        {
            var systemWorkspace = writer.SoftwareSystemWorkspace;
            var systemSyntaxTree = interfaceDeclaration.SyntaxTree;
            var systemProject = systemWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == systemSyntaxTree.FilePath));
            var systemCompilation = systemProject.GetCompilationAsync().Result;
            var systemSemanticModel = systemCompilation.GetSemanticModel(systemSyntaxTree);

            var architectureNamespace = writer.ArchitectureNamespace;

            var entityName = interfaceDeclaration.Identifier.Text;
            var entityTypeSymbol = systemSemanticModel.GetDeclaredSymbol(interfaceDeclaration) as ITypeSymbol;
            var entityAlias = AnyCodeWriter.GetEntityAlias(architectureNamespace, softwareSystemName, containerName, entityName);
            var documentName = $"{entityName}.yaml";

            var fileDirectory = Path.Combine(writer.ArchitectureOutputPath, NetToAnyCodeGenerator<YamlCodeWriter>.GetEntitiesDirectory(softwareSystemName, containerName));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                writer.AddEntityTypeMapping(entityTypeSymbol, entityAlias); 
                return interfaceDeclaration;
            }

            var sourceCode = NetToAnyCodeGenerator<YamlCodeWriter>.GetEntityCode(
                architectureNamespace,
                softwareSystemName,
                containerName,
                entityName,
                YamlCodeWriter.GetLabel(entityName));

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, sourceCode);
            }

            writer.AddEntityTypeMapping(entityTypeSymbol, entityAlias);
            return interfaceDeclaration;

        }

        public static RecordDeclarationSyntax AddEntityYamlFile(this RecordDeclarationSyntax recordDeclaration, string softwareSystemName, string containerName, NetToYamlArchitectureAsCodeWriter writer)
        {
            var systemWorkspace = writer.SoftwareSystemWorkspace;
            var systemSyntaxTree = recordDeclaration.SyntaxTree;
            var systemProject = systemWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == systemSyntaxTree.FilePath));
            var systemCompilation = systemProject.GetCompilationAsync().Result;
            var systemSemanticModel = systemCompilation.GetSemanticModel(systemSyntaxTree);

            var architectureNamespace = writer.ArchitectureNamespace;

            var entityName = recordDeclaration.Identifier.Text;
            var entityTypeSymbol = systemSemanticModel.GetDeclaredSymbol(recordDeclaration) as ITypeSymbol;
            var entityAlias = AnyCodeWriter.GetEntityAlias(architectureNamespace,softwareSystemName,containerName, entityName);

            var documentName = $"{entityName}.yaml";

            var fileDirectory = Path.Combine(writer.ArchitectureOutputPath, NetToAnyCodeGenerator<YamlCodeWriter>.GetEntitiesDirectory(softwareSystemName, containerName));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                writer.AddEntityTypeMapping(entityTypeSymbol, entityAlias);
                return recordDeclaration;
            }

            var sourceCode = NetToAnyCodeGenerator<YamlCodeWriter>.GetEntityCode(
                architectureNamespace,
                softwareSystemName,
                containerName,
                entityName,
                YamlCodeWriter.GetLabel(entityName));


            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, sourceCode);
            }

            writer.AddEntityTypeMapping(entityTypeSymbol, entityAlias);
            return recordDeclaration;

        }
        /*
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
        */

        public static ClassDeclarationSyntax AddComponentYamlFile(this ClassDeclarationSyntax classDeclaration, string softwareSystemName, string containerName, NetToYamlArchitectureAsCodeWriter writer)
        {
            var architectureNamespace = writer.ArchitectureNamespace;
            var componentName = classDeclaration.Identifier.Text;
            var documentName = $"{componentName}.yaml";

            var fileDirectory = Path.Combine(writer.ArchitectureOutputPath, NetToAnyCodeGenerator<YamlCodeWriter>.GetComponentsDirectory(softwareSystemName, containerName)); ;
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                return classDeclaration;
            }

            var sourceCode = NetToAnyCodeGenerator<YamlCodeWriter>.GetComponentCode(
                architectureNamespace,
                softwareSystemName,
                containerName,
                componentName,
                YamlCodeWriter.GetLabel(componentName));

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, sourceCode);
            }

            return classDeclaration;

        }

        public static InterfaceDeclarationSyntax AddComponentYamlFile(this InterfaceDeclarationSyntax interfaceDeclaration, string softwareSystemName, string containerName, NetToYamlArchitectureAsCodeWriter writer)
        {
            var architectureNamespace = writer.ArchitectureNamespace;

            var componentName = interfaceDeclaration.Identifier.Text;
            var documentName = $"{componentName}.yaml";

            var fileDirectory = Path.Combine(writer.ArchitectureOutputPath, NetToAnyCodeGenerator<YamlCodeWriter>.GetComponentsDirectory(softwareSystemName, containerName));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                return interfaceDeclaration;
            }

            var sourceCode = NetToAnyCodeGenerator<YamlCodeWriter>.GetComponentCode(
                architectureNamespace,
                softwareSystemName,
                containerName,
                componentName,
                YamlCodeWriter.GetLabel(componentName));


            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, sourceCode);
            }

            return interfaceDeclaration;

        }

        /*
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
        */
    }

}
