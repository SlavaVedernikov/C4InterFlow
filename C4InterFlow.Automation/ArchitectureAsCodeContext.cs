using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Text.Json.Nodes;

namespace C4InterFlow.Automation
{
    public interface IArchitectureAsCodeContext
    {
        string GetComponentInterfaceAlias();
        string GetComponentInterfaceAlias(string filePath);
    }

    public class NetArchitectureAsCodeContext : IArchitectureAsCodeContext
    {
        private ClassDeclarationSyntax _currentClassDeclaration;
        private MSBuildWorkspace _architectureWorkspace;
        public NetArchitectureAsCodeContext(ClassDeclarationSyntax currentClassDeclaration, MSBuildWorkspace architectureWorkspace)
        {
            _currentClassDeclaration = currentClassDeclaration;
            _architectureWorkspace = architectureWorkspace;
        }

        public string GetComponentInterfaceAlias()
        {
            return _currentClassDeclaration.GetAliasFieldValue();
        }

        public string GetComponentInterfaceAlias(string filePath)
        {
            var result = string.Empty;

            var architectureClassSyntaxTree = _currentClassDeclaration.SyntaxTree;
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

    public class JObjectArchitectureAsCodeContext : IArchitectureAsCodeContext
    {
        private JObject _currentArchitectureJObject;
        public JObjectArchitectureAsCodeContext(JObject currentArchitectureJObject)
        {
            _currentArchitectureJObject = currentArchitectureJObject;
        }

        public string GetComponentInterfaceAlias()
        {
            var result = string.Empty;

            var selectedToken = _currentArchitectureJObject.SelectToken("..SoftwareSystems.*.Containers.*.Components.*.Interfaces.*");

            if (selectedToken != null)
            {
                result = selectedToken.Path;
            }

            return result;
        }

        public string GetComponentInterfaceAlias(string filePath)
        {
            var result = string.Empty;

            var jsonObject = GetJsonObjectFromFile(filePath);
            var selectedToken = jsonObject.SelectToken("..SoftwareSystems.*.Containers.*.Components.*.Interfaces.*");

            if (selectedToken != null)
            {
                result = selectedToken.Path;
            }

            return result;
        }

        private JObject GetJsonObjectFromFile(string filePath)
        {
            var yaml = File.ReadAllText(filePath);

            return GetJsonObjectFromYaml(yaml);
        }

        private JObject GetJsonObjectFromYaml(string yaml)
        {
            var deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
            var yamlObject = deserializer.Deserialize(yaml);

            var serializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();

            string json = serializer.Serialize(yamlObject);

            return JObject.Parse(json);
        }
    }


}
