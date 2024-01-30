using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Text.Json.Nodes;
using C4InterFlow.Elements;
using C4InterFlow.Elements.Interfaces;
using System.Reflection;
using System.Runtime.Loader;

namespace C4InterFlow.Automation
{
    public class NetArchitectureAsCodeReaderStrategy : ArchitectureAsCodeReaderStrategy<NetElementsResolver>
    {
        private ClassDeclarationSyntax? CurrentClassDeclaration { get; set; }
        private MSBuildWorkspace? ArchitectureWorkspace { get; set; }

        public NetArchitectureAsCodeReaderStrategy()
        {

        }
        public NetArchitectureAsCodeReaderStrategy(ClassDeclarationSyntax currentClassDeclaration, MSBuildWorkspace architectureWorkspace) : this()
        {
            CurrentClassDeclaration = currentClassDeclaration;
            ArchitectureWorkspace = architectureWorkspace;
        }

        protected override NetElementsResolver ElementsResolver { get => new NetElementsResolver(); }

        public override string GetComponentInterfaceAlias()
        {
            return CurrentClassDeclaration?.GetAliasFieldValue();
        }

        public override string GetComponentInterfaceAlias(string filePath)
        {
            var result = string.Empty;

            var architectureClassSyntaxTree = CurrentClassDeclaration.SyntaxTree;
            var architectureProject = ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == architectureClassSyntaxTree.FilePath));
            var architectureCompilation = architectureProject.GetCompilationAsync().Result;
            var architectureSemanticModel = architectureCompilation.GetSemanticModel(architectureClassSyntaxTree);

            var interfaceClassSyntaxTree = architectureCompilation.SyntaxTrees.FirstOrDefault(x => x.FilePath == filePath);
            var interfaceClassSemanticModel = architectureCompilation.GetSemanticModel(interfaceClassSyntaxTree);
            var interfaceAliasField = interfaceClassSyntaxTree.GetRoot().DescendantNodes()
                .OfType<FieldDeclarationSyntax>().First(f => f.Declaration.Variables.Any(v => v.Identifier.Text == "ALIAS"));

            result = interfaceClassSemanticModel.GetConstantValue(
                interfaceAliasField.Declaration.Variables[0].Initializer.Value).Value as string;

            return result;
        }

        public override IEnumerable<Interface> GetAllInterfaces()
        {
            var result = new List<Interface>();

            var interfaceClasses = GetAllTypesOfInterface<IInterfaceInstance>();

            foreach (var interfaceClass in interfaceClasses)
            {
                var interfaceInstance = interfaceClass?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null, null) as Interface;

                if (interfaceInstance != null)
                {
                    result.Add(interfaceInstance);
                }
            }

            return result;
        }
        private IEnumerable<Type> GetAllTypesOfInterface<T>()
        {
            var result = new List<Type>();

            var assemblies = GetAssemblies();
            foreach (var assembly in assemblies)
            {
                result.AddRange(assembly
                .GetTypes()
                .Where(type => typeof(T).IsAssignableFrom(type) && !type.IsInterface));
            }
            return result;
        }

        private IEnumerable<Assembly> GetAssemblies()
        {
            var paths = Directory.GetFiles(AppContext.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly);
            //TODO: Review this logic. Consider uising inclusion logic instead of exclusion.
            return paths
            .Where(x => { var assembly = x.Split("\\").Last(); return new[] { "C4InterFlow", "System.", "Microsoft." }.All(y => !assembly.StartsWith(y)); })
            .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath);
        }
    }
}
