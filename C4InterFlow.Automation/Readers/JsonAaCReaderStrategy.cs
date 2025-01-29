using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using Newtonsoft.Json.Schema;
using System.Net.Http.Json;

namespace C4InterFlow.Automation.Readers
{
    public class JsonAaCReaderStrategy : JObjectAaCReaderStrategy
    {
        public JsonAaCReaderStrategy() : base() { }
        public JsonAaCReaderStrategy(JObject rootJObject) : base(rootJObject) { }
        
        protected override JObject GetJsonObjectFromFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JObject.Parse(json);
        }

        private bool ValidateFiles(IEnumerable<string> paths)
        {
            var result = true;
            var schema = LoadSchema();

            foreach (var path in paths)
            {
                string[] jsonFiles = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);

                foreach (string jsonFile in jsonFiles)
                {
                    var jsonContent = File.ReadAllText(jsonFile);

                    try
                    {
                        var json = JObject.Parse(jsonContent);

                        // Validate JSON against the schema
                        var isValid = json.IsValid(schema, out IList<ValidationError> errors);

                        if (!isValid)
                        {
                            foreach (var error in errors)
                            {
                                Log.Error("File {YamlFile} has error at path {Path}: {Error}", jsonFile, error.Path, error.Message);
                            }

                            result = false;
                        }
                    }
                    catch (JsonReaderException ex)
                    {
                        Log.Error(ex, "File {File} has error at line {LineNumber}, column {LinePosition}: {Error}", jsonFile, ex.LineNumber, ex.LinePosition, ex.Message);
                        result = false;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "File {File} has error: {Error}", jsonFile, ex.Message);
                        result = false;
                    }
                }
            }

            return result;
        }

        protected override JObject GetJsonObjectFromFiles(string[] aacPaths, string[] viewsPaths)
        {
            var paths = new List<string>(aacPaths);
            if(viewsPaths != null)
                paths.AddRange(viewsPaths);

            if (!ValidateFiles(paths))
            {
                throw new InvalidDataException("Input file(s) have errors.");
            }

            JObject rootNode = null;

            foreach (var path in paths)
            {
                string[] jsonFiles = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);

                foreach (string jsonFile in jsonFiles)
                {
                    using (StreamReader reader = new StreamReader(jsonFile))
                    {
                        var json = reader.ReadToEnd();
                        var jsonObject = JObject.Parse(json);

                        if (rootNode == null)
                        {
                            rootNode = jsonObject;
                        }
                        else
                        {
                            MergeNodes(rootNode, jsonObject);
                        }
                    }
                }
            }

            return rootNode;
        }

        private void MergeNodes(JObject target, JObject source)
        {
            foreach (var sourceProperty in source.Properties())
            {
                JProperty targetProperty = target.Property(sourceProperty.Name);

                if (targetProperty == null)
                {
                    // If the property doesn't exist in the target, add it
                    target.Add(sourceProperty);
                }
                else
                {
                    // If the property is a JObject, merge it recursively
                    if (sourceProperty.Value is JObject sourceValue && targetProperty.Value is JObject targetValue)
                    {
                        MergeNodes(targetValue, sourceValue);
                    }
                    else
                    {
                        // Otherwise, replace the value in the target with the value in the source
                        targetProperty.Value = sourceProperty.Value;
                    }
                }
            }
        }
    }


}
