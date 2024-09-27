using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using C4InterFlow.Structures;
using Serilog;

namespace C4InterFlow.Automation.Writers
{
    public abstract class CSharpToAnyAaCWriter : IAaCWriter
    {
        public MSBuildWorkspace SoftwareSystemWorkspace { get; private set; }
        public string ArchitectureNamespace { get; private set; }
        public Solution? SoftwareSystemSolution { get; private set; }
        public Project? CurrentProject { get; private set; }
        public Document? CurrentDocument { get; private set; }
        public ClassDeclarationSyntax? CurrentClassDeclaration { get; private set; }
        public Dictionary<string, IList<string>> SoftwareSystemTypeMap { get; private set; } = new Dictionary<string, IList<string>>();
        public Dictionary<string, string> EntityTypeMap { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, MethodDeclarationSyntax> ComponentInterfaceAaCFileToCSharpMethodDeclarationMap { get; private set; } = new Dictionary<string, MethodDeclarationSyntax>();
        public Dictionary<string, PropertyDeclarationSyntax> ComponentInterfaceAaCFileToCSharpPropertyDeclarationMap { get; private set; } = new Dictionary<string, PropertyDeclarationSyntax>();

        private static void RegisterInstanceVisualStudioInstance()
        {
            var instance = MSBuildLocator.QueryVisualStudioInstances().MaxBy(instance => instance.Version);
            if (instance != null)
            {
                MSBuildLocator.RegisterInstance(instance);
            }
            else
            {
                Log.Warning("MSBuildLocator could not find any instances to register.");
            }
        }

        public CSharpToAnyAaCWriter(string softwareSystemSourcePath, string architectureRootNamespace)
        {
            Log.Information("Loading Software System source from {Path}", softwareSystemSourcePath);
            RegisterInstanceVisualStudioInstance();
            var softwareSystemWorkspace = MSBuildWorkspace.Create(new Dictionary<string, string>()
            {
                { "BuildingInsideVisualStudio", "true" }
            });

            ArchitectureNamespace = architectureRootNamespace.Trim();
            SoftwareSystemWorkspace = softwareSystemWorkspace;

            if (softwareSystemSourcePath.EndsWith(".sln"))
            {
                SoftwareSystemSolution = softwareSystemWorkspace.OpenSolutionAsync(softwareSystemSourcePath).Result;
            }
            else if (softwareSystemSourcePath.EndsWith(".csproj"))
            {
                CurrentProject = softwareSystemWorkspace.OpenProjectAsync(softwareSystemSourcePath).Result;
                SoftwareSystemSolution = CurrentProject.Solution;
            }

        }

        public void AddSoftwareSystemTypeMapping(ITypeSymbol interfaceType, ITypeSymbol implementationType)
        {
            if (interfaceType == null || implementationType == null)
                return;

            var key = interfaceType.ToDisplayString();
            var value = implementationType.ToDisplayString();

            if (!SoftwareSystemTypeMap.Keys.Contains(key))
            {
                SoftwareSystemTypeMap.Add(key, new List<string>());
            }

            if (SoftwareSystemTypeMap.GetValueOrDefault(key)?.Contains(value) == false)
            {
                SoftwareSystemTypeMap.GetValueOrDefault(key)?.Add(value);
            }

            // This is needed for internal invocations within the implementation type
            if (!key.Equals(value))
            {
                if (!SoftwareSystemTypeMap.Keys.Contains(value))
                {
                    SoftwareSystemTypeMap.Add(value, new List<string>());
                }

                if (SoftwareSystemTypeMap.GetValueOrDefault(value)?.Contains(value) == false)
                {
                    SoftwareSystemTypeMap.GetValueOrDefault(value)?.Add(value);
                }
            }
        }

        public void AddEntityTypeMapping(ITypeSymbol entityType, string antityAlias)
        {
            var key = entityType.ToDisplayString();

            if (!EntityTypeMap.Keys.Contains(key))
            {
                EntityTypeMap.Add(key, antityAlias);
            }
        }

        public void AddSoftwareSystemTypeMapping(Type interfaceType, Type implementationType)
        {
            AddSoftwareSystemTypeMapping(interfaceType.FullName, implementationType.FullName);
        }

        public void AddSoftwareSystemTypeMapping(string? interfaceTypeName, string? implementationTypeName)
        {
            if (string.IsNullOrEmpty(interfaceTypeName) || string.IsNullOrEmpty(implementationTypeName)) return;

            ITypeSymbol? interfaceTypeSymbol = null;
            ITypeSymbol? implementationTypeSymbol = null;

            foreach (var project in SoftwareSystemSolution.Projects)
            {
                if (interfaceTypeSymbol != null && implementationTypeSymbol != null)
                    break;

                var compilation = project.GetCompilationAsync().Result;

                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    if (interfaceTypeSymbol != null && implementationTypeSymbol != null)
                        break;

                    if (interfaceTypeSymbol == null)
                    {
                        interfaceTypeSymbol = compilation.GetTypeByMetadataName(interfaceTypeName) as ITypeSymbol;
                    }

                    if (implementationTypeSymbol == null)
                    {
                        implementationTypeSymbol = compilation.GetTypeByMetadataName(implementationTypeName) as ITypeSymbol;
                        if (implementationTypeSymbol?.AllInterfaces.Length > 0)
                        {
                            interfaceTypeSymbol = implementationTypeSymbol.AllInterfaces[0];
                        }
                    }
                }
            }

            AddSoftwareSystemTypeMapping(interfaceTypeSymbol, implementationTypeSymbol);
        }

        public CSharpToAnyAaCWriter WithSoftwareSystemProject(string projectName)
        {
            CurrentProject = SoftwareSystemSolution.Projects.FirstOrDefault(x => x.Name == projectName);

            return this;
        }

        public Dictionary<string, string> GetSoftwareSystemTypeMappings(string typeName, string methodName, string[] mappingMethodNames)
        {
            var result = new Dictionary<string, string>();

            TypeDeclarationSyntax typeDeclaration = null;

            foreach (var project in SoftwareSystemSolution.Projects)
            {
                if (typeDeclaration != null)
                {
                    break;
                }

                var compilation = project.GetCompilationAsync().Result;

                foreach (var syntaxTree in compilation.SyntaxTrees)
                {
                    var classes = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
                    typeDeclaration = classes.FirstOrDefault(c =>
                    {
                        var semanticModel = compilation.GetSemanticModel(c.SyntaxTree);
                        var declaredSymbol = semanticModel.GetDeclaredSymbol(c);

                        return declaredSymbol.ToDisplayString() == typeName;
                    });

                    if (typeDeclaration != null)
                    {

                        var methodDeclaration = typeDeclaration.Members
                            .OfType<MethodDeclarationSyntax>()
                            .FirstOrDefault(m => m.Identifier.Text == methodName);

                        if (methodDeclaration != null)
                        {
                            // Find the invocation expressions within the method
                            var invocations = methodDeclaration.DescendantNodes()
                                .OfType<InvocationExpressionSyntax>();

                            foreach (var invocation in invocations)
                            {
                                // Check if the invocation is one of the specified mapping methods
                                if (mappingMethodNames.Any(x => invocation.Expression.ToString().Contains($".{x}")))
                                {
                                    var semanticModel = compilation.GetSemanticModel(invocation.SyntaxTree);

                                    // Get the symbol info for the method
                                    var methodSymbolInfo = semanticModel.GetSymbolInfo(invocation.Expression);
                                    var methodSymbol = (IMethodSymbol)(methodSymbolInfo.Symbol ?? methodSymbolInfo.CandidateSymbols.FirstOrDefault());

                                    // Get the type arguments
                                    if (methodSymbol != null)
                                    {
                                        var typeArguments = methodSymbol.TypeArguments;

                                        if (typeArguments.Count() == 2)
                                        {
                                            var interfaceName = typeArguments[0].ToDisplayString();
                                            var implementationName = typeArguments[1].ToDisplayString();

                                            if (!result.ContainsKey(interfaceName))
                                            {
                                                result.Add(interfaceName, implementationName);
                                            }

                                        }
                                    }
                                }
                            }
                        }



                        break;
                    }
                }

            }


            return result;
        }

        public IEnumerable<Document>? WithDocuments()
        {
            return CurrentProject?.Documents.Where(x => !x.FilePath.Contains(@"\obj\"));
        }

        public virtual IAaCWriter AddActor(string name, string type, string? label = null)
        {
            return this;
        }

        public virtual IAaCWriter AddBusinessProcess(string name, string? label = null)
        {
            return this;
        }

        public virtual IAaCWriter AddSoftwareSystem(string name, string? boundary = null, string? label = null, string? description = null)
        {
            return this;
        }

        public virtual IAaCWriter AddSoftwareSystemInterface(
            string softwareSystemName, 
            string name, 
            string? label = null,
            string? description = null,
            string? input = null, 
            string? output = null, 
            string? protocol = null, 
            string? path = null)
        {
            return this;
        }

        public virtual IAaCWriter AddContainer(string softwareSystemName, string name, string? containerType = null, string? label = null, string? description = null)
        {
            return this;
        }

        public virtual IAaCWriter AddContainerInterface(
            string softwareSystemName, 
            string containerName, 
            string name,
            string? label = null,
            string? description = null,
            string? input = null, 
            string? output = null, 
            string? protocol = null, 
            string? path = null)
        {
            return this;
        }

        public virtual IAaCWriter AddComponent(string softwareSystemName, string containerName, string name, ComponentType componentType = ComponentType.None)
        {
            return this;
        }

        public virtual IAaCWriter AddComponentInterface(
            string softwareSystemName,
            string containerName,
            string componentName,
            string name,
            string? label = null,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null)
        {
            return this;
        }

        public virtual string? GetComponentInterfaceAlias(string filePathPattern)
        {
            return string.Empty;
        }

        public virtual string GetFileExtension()
        {
            return string.Empty;
        }
    }
}
