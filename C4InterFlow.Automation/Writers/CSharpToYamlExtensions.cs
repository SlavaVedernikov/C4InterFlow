using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace C4InterFlow.Automation.Writers
{
    public static class CSharpToYamlExtensions
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

        public static MethodDeclarationSyntax AddComponentInterfaceYamlFile(this MethodDeclarationSyntax methodDeclaration,
            string softwareSystemName, string containerName, string componentName, CSharpToYamlAaCWriter writer,
            Func<MethodDeclarationSyntax, SemanticModel, CSharpToYamlAaCWriter, string?, string?, string?, string>? pathMapper = null,
            string? protocol = null)
        {
            var architectureNamespace = writer.ArchitectureNamespace;
            var systemSyntaxTree = methodDeclaration.SyntaxTree;
            var systemProject = writer.SoftwareSystemWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == systemSyntaxTree.FilePath));
            var systemCompilation = systemProject.GetCompilationAsync().Result;
            var systemSemanticModel = systemCompilation.GetSemanticModel(systemSyntaxTree);

            var interfaceName = methodDeclaration.Identifier.Text;
            var documentName = $"{interfaceName}.yaml";

            var fileDirectory = Path.Combine(writer.ArchitectureOutputPath, CSharpToAnyCodeGenerator<YamlCodeWriter>.GetComponentInterfacesDirectory(softwareSystemName, containerName, componentName));
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

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
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

            var sourceCode = CSharpToAnyCodeGenerator<YamlCodeWriter>.GetComponentInterfaceCode(
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
            string softwareSystemName, string containerName, string componentName, CSharpToYamlAaCWriter writer,
            string[] interfaces,
            string? protocol = null)
        {
            var architectureNamespace = writer.ArchitectureNamespace;

            var systemSyntaxTree = propertyDeclaration.SyntaxTree;
            var systemProject = writer.SoftwareSystemWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == systemSyntaxTree.FilePath));
            var systemCompilation = systemProject.GetCompilationAsync().Result;
            var systemSemanticModel = systemCompilation.GetSemanticModel(systemSyntaxTree);

            foreach (var @interface in interfaces)
            {
                var interfaceName = $"{propertyDeclaration.Identifier.Text}{@interface}";
                var documentName = $"{interfaceName}.yaml";

                var fileDirectory = Path.Combine(writer.ArchitectureOutputPath, CSharpToAnyCodeGenerator<YamlCodeWriter>.GetComponentInterfacesDirectory(softwareSystemName, containerName, componentName));
                var filePath = Path.Combine(fileDirectory, documentName);

                Directory.CreateDirectory(fileDirectory);

                if (!writer.ComponentInterfaceAaCFileToCSharpPropertyDeclarationMap.Keys.Contains(filePath))
                {
                    writer.ComponentInterfaceAaCFileToCSharpPropertyDeclarationMap.Add(filePath, propertyDeclaration);
                }

                if (File.Exists(filePath))
                {
                    Console.WriteLine($"Document '{filePath}' already exists.");
                    continue;
                }

                var sourceCode = CSharpToAnyCodeGenerator<YamlCodeWriter>.GetComponentInterfaceCode(
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
        public static InterfaceDeclarationSyntax AddEntityYamlFile(this InterfaceDeclarationSyntax interfaceDeclaration, string softwareSystemName, string containerName, CSharpToYamlAaCWriter writer)
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

            var fileDirectory = Path.Combine(writer.ArchitectureOutputPath, CSharpToAnyCodeGenerator<YamlCodeWriter>.GetEntitiesDirectory(softwareSystemName, containerName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                writer.AddEntityTypeMapping(entityTypeSymbol, entityAlias);
                return interfaceDeclaration;
            }

            var sourceCode = CSharpToAnyCodeGenerator<YamlCodeWriter>.GetEntityCode(
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

        public static RecordDeclarationSyntax AddEntityYamlFile(this RecordDeclarationSyntax recordDeclaration, string softwareSystemName, string containerName, CSharpToYamlAaCWriter writer)
        {
            var systemWorkspace = writer.SoftwareSystemWorkspace;
            var systemSyntaxTree = recordDeclaration.SyntaxTree;
            var systemProject = systemWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == systemSyntaxTree.FilePath));
            var systemCompilation = systemProject.GetCompilationAsync().Result;
            var systemSemanticModel = systemCompilation.GetSemanticModel(systemSyntaxTree);

            var architectureNamespace = writer.ArchitectureNamespace;

            var entityName = recordDeclaration.Identifier.Text;
            var entityTypeSymbol = systemSemanticModel.GetDeclaredSymbol(recordDeclaration) as ITypeSymbol;
            var entityAlias = AnyCodeWriter.GetEntityAlias(architectureNamespace, softwareSystemName, containerName, entityName);

            var documentName = $"{entityName}.yaml";

            var fileDirectory = Path.Combine(writer.ArchitectureOutputPath, CSharpToAnyCodeGenerator<YamlCodeWriter>.GetEntitiesDirectory(softwareSystemName, containerName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                writer.AddEntityTypeMapping(entityTypeSymbol, entityAlias);
                return recordDeclaration;
            }

            var sourceCode = CSharpToAnyCodeGenerator<YamlCodeWriter>.GetEntityCode(
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

        public static ClassDeclarationSyntax AddComponentYamlFile(this ClassDeclarationSyntax classDeclaration, string softwareSystemName, string containerName, CSharpToYamlAaCWriter writer)
        {
            var architectureNamespace = writer.ArchitectureNamespace;
            var componentName = classDeclaration.Identifier.Text;
            var documentName = $"{componentName}.yaml";

            var fileDirectory = Path.Combine(writer.ArchitectureOutputPath, CSharpToAnyCodeGenerator<YamlCodeWriter>.GetComponentsDirectory(softwareSystemName, containerName)); ;
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                return classDeclaration;
            }

            var sourceCode = CSharpToAnyCodeGenerator<YamlCodeWriter>.GetComponentCode(
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

        public static InterfaceDeclarationSyntax AddComponentYamlFile(this InterfaceDeclarationSyntax interfaceDeclaration, string softwareSystemName, string containerName, CSharpToYamlAaCWriter writer)
        {
            var architectureNamespace = writer.ArchitectureNamespace;

            var componentName = interfaceDeclaration.Identifier.Text;
            var documentName = $"{componentName}.yaml";

            var fileDirectory = Path.Combine(writer.ArchitectureOutputPath, CSharpToAnyCodeGenerator<YamlCodeWriter>.GetComponentsDirectory(softwareSystemName, containerName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                return interfaceDeclaration;
            }

            var sourceCode = CSharpToAnyCodeGenerator<YamlCodeWriter>.GetComponentCode(
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
    }

}
