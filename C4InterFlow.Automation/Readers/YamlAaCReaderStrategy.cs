using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Reflection;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace C4InterFlow.Automation.Readers
{
    public class YamlAaCReaderStrategy : JObjectAaCReaderStrategy
    {
        public YamlAaCReaderStrategy() : base() { }
        public YamlAaCReaderStrategy(JObject rootJObject) : base(rootJObject) { }
        
        protected override JObject GetJsonObjectFromFile(string filePath)
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

        protected override JObject GetJsonObjectFromFiles(string[] paths)
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
