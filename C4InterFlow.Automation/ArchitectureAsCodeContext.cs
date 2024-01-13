using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Newtonsoft.Json.Linq;

namespace C4InterFlow.Automation
{
    public interface IArchitectureAsCodeContext
    {
        string GetAliasFieldValue();
        string GetAliasFieldValue(string filePath);
    }

    public class NetArchitectureAsCodeContext : IArchitectureAsCodeContext
    {
        private ClassDeclarationSyntax _architectureClassDeclaration;
        private MSBuildWorkspace _architectureWorkspace;
        public NetArchitectureAsCodeContext(ClassDeclarationSyntax architectureClassDeclaration, MSBuildWorkspace architectureWorkspace)
        {
            _architectureClassDeclaration = architectureClassDeclaration;
            _architectureWorkspace = architectureWorkspace;
        }

        public string GetAliasFieldValue()
        {
            return _architectureClassDeclaration.GetAliasFieldValue();
        }

        public string GetAliasFieldValue(string filePath)
        {
            var result = string.Empty;

            var architectureClassSyntaxTree = _architectureClassDeclaration.SyntaxTree;
            var architectureProject = _architectureWorkspace.CurrentSolution.Projects.FirstOrDefault(p => p.Documents.Any(d => d.FilePath == architectureClassSyntaxTree.FilePath));
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
    }

    public class JsonArchitectureAsCodeContext : IArchitectureAsCodeContext
    {
        private JObject _architectureJObject;
        public JsonArchitectureAsCodeContext(JObject architectureJObject)
        {
            _architectureJObject = architectureJObject;
        }

        public string GetAliasFieldValue()
        {
            var result = string.Empty;

            return result;
        }

        public string GetAliasFieldValue(string filePath)
        {
            var result = string.Empty;

            return result;
        }
    }


}
