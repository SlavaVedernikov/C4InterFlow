using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Text;
using Serilog;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Core;
using Newtonsoft.Json.Schema;
using System.Text.RegularExpressions;

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

            var schema = LoadSchema();

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
                        FormatScalarTypes(json);

                        // Validate JSON against the schema
                        var isValid = json.IsValid(schema, out IList<ValidationError> errors);

                        if (!isValid)
                        {
                            foreach (var error in errors)
                            {
                                Log.Error("File {YamlFile} has error at path {Path}: {Error}", yamlFile, error.Path, error.Message);
                            }

                            result = false;
                        }
                        
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

        private static IEnumerable<string> GetAllPaths(JToken token, string parentPath = "")
        {
            if (token is JObject jObject)
            {
                foreach (var property in jObject.Properties())
                {
                    var currentPath = string.IsNullOrEmpty(parentPath)
                        ? property.Name
                        : $"{parentPath}.{property.Name}";

                    // Recurse into children
                    foreach (var path in GetAllPaths(property.Value, currentPath))
                    {
                        yield return path;
                    }
                }
            }
            else if (token is JArray jArray)
            {
                for (int i = 0; i < jArray.Count; i++)
                {
                    var currentPath = $"{parentPath}[{i}]";

                    // Recurse into children
                    foreach (var path in GetAllPaths(jArray[i], currentPath))
                    {
                        yield return path;
                    }
                }
            }
            else
            {
                // Base case: return the current path
                yield return parentPath;
            }
        }

        private static void FormatScalarTypes(JObject json)
        {
            foreach (var nodePath in GetAllPaths(json))
            {
                var token = json.SelectToken(nodePath);
                if (bool.TryParse(token.Value<string>(), out var boolValue) &&
                    Regex.IsMatch(token.Path, @"^.*\.Views\..*\.ExpandUpstream$"))
                {
                    token.Replace(boolValue);
                }
                else if (int.TryParse(token.Value<string>(), out var intValue) &&
                    Regex.IsMatch(token.Path, @"^.*\.Views\..*\.MaxLineLabels$"))
                {
                    token.Replace(intValue);
                }
            }
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
