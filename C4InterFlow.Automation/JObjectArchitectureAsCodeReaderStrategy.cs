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
using System.Text;
using YamlDotNet.RepresentationModel;

namespace C4InterFlow.Automation
{
    public class JObjectArchitectureAsCodeReaderStrategy : ArchitectureAsCodeReaderStrategy<JObjectElementsResolver>
    {
        private JObject? RootJObject { get; set; }
        public JObjectArchitectureAsCodeReaderStrategy()
        {
           
        }
        public JObjectArchitectureAsCodeReaderStrategy(JObject rootJObject)
        {
            RootJObject = rootJObject;
        }

        public override void Initialise(string[]? architectureInputPaths, Dictionary<string, string>? parameters)
        {
            RootJObject = GetJsonObjectFromFiles(architectureInputPaths);
            base.Initialise(architectureInputPaths, parameters);
        }

        public override JObjectElementsResolver ElementsResolver { get => new JObjectElementsResolver(RootJObject ?? new JObject()); }

        public override string GetComponentInterfaceAlias()
        {
            var result = string.Empty;

            var selectedToken = RootJObject.SelectToken("..SoftwareSystems.*.Containers.*.Components.*.Interfaces.*");

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

        private JObject GetJsonObjectFromFiles(string[] paths)
        {
            YamlMappingNode rootNode = null;

            foreach (var path in paths)
            {
                string[] yamlFiles = Directory.GetFiles(path, "*.yaml", SearchOption.AllDirectories);
                
                foreach (string yamlFile in yamlFiles)
                {
                    using (StreamReader reader = new StreamReader(yamlFile))
                    {
                        var yamlStream = new YamlStream();
                        yamlStream.Load(reader);

                        if (rootNode == null)
                        {
                            rootNode = (YamlMappingNode)yamlStream.Documents[0].RootNode;
                        }
                        else
                        {
                            MergeNodes(rootNode, (YamlMappingNode)yamlStream.Documents[0].RootNode);
                        }
                    }
                }
            }

            var document = new YamlDocument(rootNode);

            return GetJObject(document);
        }

        private JObject GetJObject(YamlDocument yamlDocument)
        {
            var yamlStream = new YamlStream(yamlDocument);
            var stringBuilder = new StringBuilder();

            using (var stringWriter = new StringWriter(stringBuilder))
            {
                // Pass 'false' to avoid getting random strings in the output
                yamlStream.Save(stringWriter, false);
            }

            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize(new StringReader(stringBuilder.ToString()));

            var serializer = new SerializerBuilder().JsonCompatible().Build();
            string jsonString = serializer.Serialize(yamlObject);

            return JObject.Parse(jsonString);
        }

        private void MergeNodes(YamlMappingNode target, YamlMappingNode source)
        {
            foreach (var sourceEntry in source.Children)
            {
                if (!target.Children.ContainsKey(sourceEntry.Key))
                {
                    target.Children.Add(sourceEntry.Key, sourceEntry.Value);
                }
                else
                {
                    if (sourceEntry.Value is YamlMappingNode sourceValueMapping && target.Children[sourceEntry.Key] is YamlMappingNode targetValueMapping)
                    {
                        MergeNodes(targetValueMapping, sourceValueMapping);
                    }
                    else
                    {
                        target.Children[sourceEntry.Key] = sourceEntry.Value;
                    }
                }
            }
        }
    }


}
