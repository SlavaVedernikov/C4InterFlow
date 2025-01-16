using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Text;
using Serilog;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Core;
using Newtonsoft.Json.Schema;
using System.Reflection;

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

        private bool ValidateFiles(IEnumerable<string> paths)
        {
            var result = true;

            // Load the embedded JSON schema resource
            var assembly = typeof(Structures.Structure).Assembly;
            var resourceName = "C4InterFlow.schema.json";

            string schemaContent;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    schemaContent = reader.ReadToEnd();
                }
            }

            var schema = JSchema.Parse(schemaContent);

            foreach (var path in paths)
            {
                string[] yamlFiles = Directory.GetFiles(path, "*.yaml", SearchOption.AllDirectories);

                foreach (string yamlFile in yamlFiles)
                {
                    var yaml = File.ReadAllText(yamlFile);
                    var input = new StringReader(yaml);
                    var deserializer = new DeserializerBuilder().Build();
                    try
                    {
                        var yamlObject = deserializer.Deserialize(input);

                        var serializer = new SerializerBuilder()
                            .JsonCompatible()
                            .Build();
                        var jsonContent = serializer.Serialize(yamlObject);

                        // Parse JSON and load schema
                        var json = JObject.Parse(jsonContent);

                        // Validate JSON against the schema
                        var isValid = json.IsValid(schema, out IList<ValidationError> errors);

                        if (!isValid)
                        {
                            foreach (var error in errors)
                            {
                                Log.Error("File {YamlFile} has error at path {Path}: {Error}", yamlFile, error.Path, error.Message);
                            }
                        }

                        result = false;
                    }
                    catch (YamlException ex)
                    {
                        Log.Error(ex, "File {YamlFile} has error at line {LineNumber}, column {ColumnNumber}: {Error}",yamlFile, ex.Start.Line, ex.Start.Column, ex.Message );
                        result = false;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("File {YamlFile} has error: {Error}", yamlFile, ex.Message);
                        result = false;
                    }
                }
            }

            return result;
        }

        protected override JObject GetJsonObjectFromFiles(string[] aacPaths, string[] viewsPaths)
        {
            var paths = new List<string>(aacPaths);
            if (viewsPaths != null)
                paths.AddRange(viewsPaths);

            if (!ValidateFiles(paths))
            {
                throw new InvalidDataException("Input file(s) have errors.");
            }

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
