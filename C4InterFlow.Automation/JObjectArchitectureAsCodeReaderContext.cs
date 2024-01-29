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
    public class JObjectArchitectureAsCodeReaderContext : ArchitectureAsCodeReaderContext<JObjectElementsResolver>
    {
        private JObject? CurrentArchitectureJObject { get; set; }
        public JObjectArchitectureAsCodeReaderContext()
        {
        }
        public JObjectArchitectureAsCodeReaderContext(JObject currentArchitectureJObject) : this()
        {
            CurrentArchitectureJObject = currentArchitectureJObject;
        }

        public override string GetComponentInterfaceAlias()
        {
            var result = string.Empty;

            var selectedToken = CurrentArchitectureJObject.SelectToken("..SoftwareSystems.*.Containers.*.Components.*.Interfaces.*");

            if (selectedToken != null)
            {
                result = selectedToken.Path;
            }

            return result;
        }

        public override string GetComponentInterfaceAlias(string filePath)
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

        public override IEnumerable<Interface> GetAllInterfaces()
        {
            var result = new List<Interface>();

            //TODO: Add logic

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
