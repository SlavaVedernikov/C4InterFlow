using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using Newtonsoft.Json.Converters;
using System.Drawing;

namespace C4InterFlow.Automation.Writers
{
    public class CsvToJsonAaCWriter : CsvToJObjectAaCWriter
    {
        private string FileExtension => "json";
        protected CsvToJsonAaCWriter(string architectureInputPath) : base(architectureInputPath)
        {
        }

        public static CsvToJsonAaCWriter WithCsvData(string csvRootPath)
        {
            return new CsvToJsonAaCWriter(csvRootPath);
        }

        public override void WriteArchitecture(string architectureOutputPath, string fileName)
        {
            if (!Directory.Exists(architectureOutputPath))
            {
                Directory.CreateDirectory(architectureOutputPath);
            }

            var filePath = Path.Combine(architectureOutputPath, $"{fileName}.{FileExtension}");

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new StringEnumConverter());

            var json = JsonConvert.SerializeObject(JsonArchitectureAsCode, Formatting.Indented, settings);
            File.WriteAllText(filePath, json);

            JsonArchitectureAsCode = new JObject();
        }
    }
}
