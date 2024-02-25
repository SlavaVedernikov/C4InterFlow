using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace C4InterFlow.Automation.Writers
{
    public class CsvToYamlAaCWriter : CsvToJObjectAaCWriter
    {
        private string FileExtension => "yaml";
        protected CsvToYamlAaCWriter(string architectureInputPath) : base(architectureInputPath)
        {
        }

        public static CsvToYamlAaCWriter WithCsvData(string csvRootPath)
        {
            return new CsvToYamlAaCWriter(csvRootPath);
        }

        public override void WriteArchitecture(string architectureOutputPath, string fileName)
        {
            if (!Directory.Exists(architectureOutputPath))
            {
                Directory.CreateDirectory(architectureOutputPath);
            }

            var filePath = Path.Combine(architectureOutputPath, $"{fileName}.{FileExtension}");
            var json = JsonConvert.SerializeObject(JsonArchitectureAsCode, Formatting.Indented);

            var jsonObject = JsonConvert.DeserializeObject<ExpandoObject>(json);
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(jsonObject);
            File.WriteAllText(filePath, yaml);

            JsonArchitectureAsCode = new JObject();
        }
    }
}
