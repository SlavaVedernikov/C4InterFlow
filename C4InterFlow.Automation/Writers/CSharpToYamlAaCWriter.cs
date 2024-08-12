using Microsoft.CodeAnalysis;
using C4InterFlow.Structures;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using Newtonsoft.Json;
using System.Dynamic;
using C4InterFlow.Automation.Readers;
using Serilog;

namespace C4InterFlow.Automation.Writers
{
    public class CSharpToYamlAaCWriter : CSharpToAnyAaCWriter
    {
        private string FileExtension => "yaml";
        public string? ArchitectureOutputPath { get; private set; }
        public CSharpToYamlAaCWriter(string softwareSystemSolutionPath, string architectureRootNamespace, string architectureOutputPath) : base(softwareSystemSolutionPath, architectureRootNamespace)
        {
            ArchitectureOutputPath = architectureOutputPath;
        }

        public override CSharpToYamlAaCWriter AddSoftwareSystem(string name, string? boundary = null, string? label = null, string? description = null)
        {
            var documentName = $"{name}.{FileExtension}";
            var fileDirectory = Path.Combine(ArchitectureOutputPath, CSharpToAnyCodeGenerator<YamlCodeWriter>.GetSoftwareSystemsDirectory());
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
            {
                Log.Warning("Document {Name} already exists", filePath);

                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<YamlCodeWriter>.GetSoftwareSystemCode(
                ArchitectureNamespace,
                name,
                YamlCodeWriter.GetLabel(name),
                description);


            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, sourceCode);
            }

            return this;
        }

        public override CSharpToYamlAaCWriter AddContainer(string softwareSystemName, string name, string? containerType = null, string? label = null, string? description = null)
        {
            var documentName = $"{name}.{FileExtension}";

            var fileDirectory = Path.Combine(ArchitectureOutputPath, CSharpToAnyCodeGenerator<YamlCodeWriter>.GetContainersDirectory(softwareSystemName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
            {
                Log.Warning("Document {Name} already exists", filePath);

                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<YamlCodeWriter>.GetContainerCode(
                ArchitectureNamespace,
                softwareSystemName,
                name,
                YamlCodeWriter.GetLabel(name),
                containerType,
                description);

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, sourceCode);
            }

            return this;
        }

        public override CSharpToYamlAaCWriter AddComponent(string softwareSystemName, string containerName, string name, ComponentType componentType = ComponentType.None)
        {
            var documentName = $"{name}.{FileExtension}";

            var fileDirectory = Path.Combine(ArchitectureOutputPath, CSharpToAnyCodeGenerator<YamlCodeWriter>.GetComponentsDirectory(softwareSystemName, containerName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
            {
                Log.Warning("Document {Name} already exists", filePath);

                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<YamlCodeWriter>.GetComponentCode(
                ArchitectureNamespace,
                softwareSystemName,
                containerName,
                name,
                YamlCodeWriter.GetLabel(name),
                componentType.ToString());

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, sourceCode);
            }

            return this;

        }

        public override CSharpToYamlAaCWriter AddComponentInterface(
            string softwareSystemName,
            string containerName,
            string componentName,
            string name,
            string? label = null,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null)
        {
            var documentName = $"{name}.{FileExtension}";

            var fileDirectory = Path.Combine(ArchitectureOutputPath, CSharpToAnyCodeGenerator<YamlCodeWriter>.GetComponentInterfacesDirectory(softwareSystemName, containerName, componentName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
            {
                Log.Warning("Document {Name} already exists", filePath);

                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<YamlCodeWriter>.GetComponentInterfaceCode(
                architectureNamespace: ArchitectureNamespace,
                softwareSystemName: softwareSystemName,
                containerName: containerName,
                componentName: componentName,
                name: name,
                label: label ?? YamlCodeWriter.GetLabel(name),
                protocol: protocol,
                path: path,
                input: input,
                output: output);

            //TODO: Add support for Interface method overloads
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, sourceCode);
            }

            return this;
        }

        public void AddFlowToComponentInterfaceYamlFile(string filePath,
            IEnumerable<CSharpToAnyMethodTriggerMapper>? methodTriggerMappers = null,
            IEnumerable<NetToAnyAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            if (!filePath.EndsWith($".{FileExtension}")) return;

            var systemMethodDeclaration = ComponentInterfaceAaCFileToCSharpMethodDeclarationMap.GetValueOrDefault(filePath);
            if (systemMethodDeclaration == null)
            {
                return;
            }

            var architectureObject = GetJsonObjectFromYamlFile(filePath); ;
            var flowCode = CSharpToAnyCodeGenerator<YamlCodeWriter>.GetFlowCode(
                systemMethodDeclaration,
                new YamlAaCReaderStrategy(architectureObject),
                this,
                alternativeInvocationMappers);

            //Remove \t characters
            //TODO: Investigate how \t is written into the Flow Code
            flowCode = flowCode.Replace("\t", string.Empty);

            var flowJsonObject = GetJsonObjectFromYaml(flowCode);

            var componentInterfaceJsonObject = architectureObject.SelectToken("..SoftwareSystems.*.Containers.*.Components.*.Interfaces.*") as JObject;

            var flows = flowJsonObject["Flow"]?["Flows"];
            if(flows != null)
            {
                componentInterfaceJsonObject["Flows"] = flows;
            }

            var json = JsonConvert.SerializeObject(architectureObject, Formatting.Indented);
            var yaml = new SerializerBuilder().Build().Serialize(JsonConvert.DeserializeObject<ExpandoObject>(json));

            File.WriteAllText(filePath, yaml);
        }

        public List<string> WithComponentInterfaceFiles()
        {
            string pattern = @$"^.*\\SoftwareSystems\\.*\\Containers\\.*\\Components\\.*\\Interfaces\\.*\.yaml$";
            List<string> result = Directory.GetFiles(ArchitectureOutputPath, $"*.{FileExtension}", SearchOption.AllDirectories)
                .Where(x => Regex.IsMatch(x, pattern)).ToList();

            return result;
        }

        private JObject GetJsonObjectFromYamlFile(string filePath)
        {
            if (!filePath.EndsWith($".{FileExtension}")) return new JObject();

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

        public override string? GetComponentInterfaceAlias(string filePathPattern)
        {
            var result = string.Empty;

            var filePath = Directory.GetFiles(ArchitectureOutputPath, $"*.{FileExtension}", SearchOption.AllDirectories)
                .FirstOrDefault(x => Regex.IsMatch(x, filePathPattern));

            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    var jsonObject = GetJsonObjectFromYamlFile(filePath);
                    var selectedToken = jsonObject.SelectToken("..SoftwareSystems.*.Containers.*.Components.*.Interfaces.*");

                    if (selectedToken != null)
                    {
                        result = selectedToken.Path;
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning("Could not get Alias from file pattern {Pattern}. Error: {Error}", filePathPattern, ex.Message);
                }
            }
            return result;
        }

        public override string GetFileExtension()
        {
            return FileExtension;
        }

    }
}
