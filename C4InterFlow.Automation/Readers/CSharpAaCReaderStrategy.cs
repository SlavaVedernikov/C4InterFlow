using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using C4InterFlow.Automation.Writers;
using System.Text;

namespace C4InterFlow.Automation.Readers
{
    public class CSharpAaCReaderStrategy : AaCReaderStrategy<NetStructuresResolver>
    {
        private ClassDeclarationSyntax? CurrentClassDeclaration { get; set; }
        private MSBuildWorkspace? ArchitectureWorkspace { get; set; }

        public CSharpAaCReaderStrategy()
        {

        }
        public CSharpAaCReaderStrategy(ClassDeclarationSyntax currentClassDeclaration, MSBuildWorkspace architectureWorkspace) : this()
        {
            CurrentClassDeclaration = currentClassDeclaration;
            ArchitectureWorkspace = architectureWorkspace;
        }

        public override void Initialise(string[]? architectureInputPaths, Dictionary<string, string>? parameters)
        {
            _elementsResolver = new NetStructuresResolver(architectureInputPaths);
            base.Initialise(architectureInputPaths, parameters);
        }

        private NetStructuresResolver _elementsResolver;
        public override NetStructuresResolver ElementsResolver { get { return _elementsResolver; } }

        public override string GetComponentInterfaceAlias()
        {
            return CurrentClassDeclaration?.GetAliasFieldValue();
        }

        public override string GetComponentInterfaceAlias(string filePath)
        {
            var result = new StringBuilder();

            var architectureClassSyntaxTree = CurrentClassDeclaration?.SyntaxTree;
            var architectureProject = ArchitectureWorkspace?.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == architectureClassSyntaxTree?.FilePath));
            var architectureCompilation = architectureProject?.GetCompilationAsync().Result;

            var interfaceClassSyntaxTree = architectureCompilation?.SyntaxTrees.FirstOrDefault(x => x.FilePath == filePath);
            var rootNode = interfaceClassSyntaxTree?.GetRoot();

            if (rootNode == null) return string.Empty;

            // Get the namespace declaration
            var namespaceDeclaration = rootNode.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            if (namespaceDeclaration != null)
            {
                result.Append(namespaceDeclaration.Name.ToString());
            }

            // Get the class declarations and build the hierarchy
            var classDeclarations = rootNode.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var classDecl in classDeclarations)
            {
                result.Append($".{classDecl.Identifier.Text}");
            }

            return result.ToString();
        }

    }
}
