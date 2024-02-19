using Microsoft.CodeAnalysis;
using C4InterFlow.Structures;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using Newtonsoft.Json;
using System.Dynamic;
using C4InterFlow.Automation.Readers;

namespace C4InterFlow.Automation.Writers
{
    public class CSharpToYamlAaCWriter : CSharpToAnyAaCWriter
    {
        public string? ArchitectureOutputPath { get; private set; }
        public CSharpToYamlAaCWriter(string softwareSystemSolutionPath, string architectureRootNamespace, string architectureOutputPath) : base(softwareSystemSolutionPath, architectureRootNamespace)
        {
            ArchitectureOutputPath = architectureOutputPath;
        }

        public override CSharpToYamlAaCWriter AddSoftwareSystem(string softwareSystemName)
        {
            var documentName = $"{softwareSystemName}.yaml";
            var fileDirectory = Path.Combine(ArchitectureOutputPath, CSharpToAnyCodeGenerator<YamlCodeWriter>.GetSoftwareSystemsDirectory());
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<YamlCodeWriter>.GetSoftwareSystemCode(
                ArchitectureNamespace,
                softwareSystemName,
                YamlCodeWriter.GetLabel(softwareSystemName));


            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, sourceCode);
            }

            return this;
        }

        public override CSharpToYamlAaCWriter AddContainer(string softwareSystemName, string containerName)
        {
            var documentName = $"{containerName}.yaml";

            var fileDirectory = Path.Combine(ArchitectureOutputPath, CSharpToAnyCodeGenerator<YamlCodeWriter>.GetContainersDirectory(softwareSystemName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<YamlCodeWriter>.GetContainerCode(
                ArchitectureNamespace,
                softwareSystemName,
                containerName,
                YamlCodeWriter.GetLabel(containerName));

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, sourceCode);
            }

            return this;
        }

        public override CSharpToYamlAaCWriter AddComponent(string softwareSystemName, string containerName, string componentName, ComponentType componentType = ComponentType.None)
        {
            var documentName = $"{componentName}.yaml";

            var fileDirectory = Path.Combine(ArchitectureOutputPath, CSharpToAnyCodeGenerator<YamlCodeWriter>.GetComponentsDirectory(softwareSystemName, containerName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<YamlCodeWriter>.GetComponentCode(
                ArchitectureNamespace,
                softwareSystemName,
                containerName,
                componentName,
                YamlCodeWriter.GetLabel(componentName),
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
            string interfaceName,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null)
        {
            var documentName = $"{interfaceName}.yaml";

            var fileDirectory = Path.Combine(ArchitectureOutputPath, CSharpToAnyCodeGenerator<YamlCodeWriter>.GetComponentInterfacesDirectory(softwareSystemName, containerName, componentName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<YamlCodeWriter>.GetComponentInterfaceCode(
                architectureNamespace: ArchitectureNamespace,
                softwareSystemName: softwareSystemName,
                containerName: containerName,
                componentName: componentName,
                name: interfaceName,
                label: YamlCodeWriter.GetLabel(interfaceName),
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
            if (!filePath.EndsWith(".yaml")) return;

            var systemMethodDeclaration = ComponentMethodInterfaceObjectMap.GetValueOrDefault(filePath);
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

            componentInterfaceJsonObject["Flow"] = flowJsonObject["Flow"];

            var json = JsonConvert.SerializeObject(architectureObject, Formatting.Indented);
            var yaml = new SerializerBuilder().Build().Serialize(JsonConvert.DeserializeObject<ExpandoObject>(json));

            File.WriteAllText(filePath, yaml);
        }

        public List<string> WithComponentInterfaceFiles()
        {
            string pattern = @"^.*\\SoftwareSystems\\.*\\Containers\\.*\\Components\\.*\\Interfaces\\.*\.yaml$";
            List<string> result = Directory.GetFiles(ArchitectureOutputPath, "*.yaml", SearchOption.AllDirectories)
                .Where(x => Regex.IsMatch(x, pattern)).ToList();

            return result;
        }

        private JObject GetJsonObjectFromYamlFile(string filePath)
        {
            if (!filePath.EndsWith(".yaml")) return new JObject();

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

            var filePath = Directory.GetFiles(ArchitectureOutputPath, "*.yaml", SearchOption.AllDirectories)
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
                    Console.WriteLine($"Could not get Alias from file pattern '{filePathPattern}'. Error: '{ex.Message}'");
                }
            }
            return result;
        }

        public override string GetFileExtension()
        {
            return "yaml";
        }

    }
}
