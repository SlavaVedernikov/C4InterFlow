using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Reflection;
using System.Text;
using YamlDotNet.RepresentationModel;

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

        protected override JObject GetJsonObjectFromFiles(string[] paths)
        {
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
