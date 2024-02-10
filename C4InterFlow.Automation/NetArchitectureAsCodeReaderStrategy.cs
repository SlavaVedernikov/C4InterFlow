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

        public override void Initialise(string[]? architectureInputPaths, Dictionary<string, string>? parameters)
        {
            _elementsResolver = new NetElementsResolver(architectureInputPaths);
            base.Initialise(architectureInputPaths, parameters);
        }

        private NetElementsResolver _elementsResolver;
        public override NetElementsResolver ElementsResolver { get { return _elementsResolver; } }

        public override string GetComponentInterfaceAlias()
        {
            return CurrentClassDeclaration?.GetAliasFieldValue();
        }

        public override string GetComponentInterfaceAlias(string filePath)
        {
            var result = string.Empty;

            var architectureClassSyntaxTree = CurrentClassDeclaration?.SyntaxTree;
            var architectureProject = ArchitectureWorkspace?.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == architectureClassSyntaxTree?.FilePath));
            var architectureCompilation = architectureProject?.GetCompilationAsync().Result;

            var interfaceClassSyntaxTree = architectureCompilation?.SyntaxTrees.FirstOrDefault(x => x.FilePath == filePath);
            var interfaceAliasField = interfaceClassSyntaxTree?.GetRoot().DescendantNodes()
                .OfType<FieldDeclarationSyntax>().First(f => f.Declaration.Variables.Any(v => v.Identifier.Text == "ALIAS"));

            result = interfaceAliasField?.Declaration?.Variables[0]?.Initializer?.Value.ToString().Replace("\"", string.Empty);

            return result ?? string.Empty;
        }
    }
}
