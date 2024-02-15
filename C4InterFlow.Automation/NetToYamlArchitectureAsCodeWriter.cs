using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using C4InterFlow.Elements;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using System.Dynamic;

namespace C4InterFlow.Automation
{
    public class NetToYamlArchitectureAsCodeWriter : NetToAnyArchitectureAsCodeWriter
    {
        public string? ArchitectureOutputPath { get; private set; }
        public NetToYamlArchitectureAsCodeWriter(string softwareSystemSolutionPath, string architectureRootNamespace, string architectureOutputPath) : base(softwareSystemSolutionPath, architectureRootNamespace)
        {
            ArchitectureOutputPath = architectureOutputPath;
        }

        public override NetToYamlArchitectureAsCodeWriter AddSoftwareSystem(string softwareSystemName)
        {
            var documentName = $"{softwareSystemName}.yaml";
            var fileDirectory = Path.Combine(ArchitectureOutputPath, NetToAnyCodeGenerator<YamlCodeWriter>.GetSoftwareSystemsDirectory());
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                return this;
            }

            var sourceCode = NetToAnyCodeGenerator<YamlCodeWriter>.GetSoftwareSystemCode(
                ArchitectureNamespace,
                softwareSystemName,
                YamlCodeWriter.GetLabel(softwareSystemName));


            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, sourceCode);
            }

            return this;
        }

        public override NetToYamlArchitectureAsCodeWriter AddContainer(string softwareSystemName, string containerName)
        {
            var documentName = $"{containerName}.yaml";

            var fileDirectory = Path.Combine(ArchitectureOutputPath, NetToAnyCodeGenerator<YamlCodeWriter>.GetContainersDirectory(softwareSystemName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                return this;
            }

            var sourceCode = NetToAnyCodeGenerator<YamlCodeWriter>.GetContainerCode(
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

        public override NetToYamlArchitectureAsCodeWriter AddComponent(string softwareSystemName, string containerName, string componentName, ComponentType componentType = ComponentType.None)
        {
            var documentName = $"{componentName}.yaml";

            var fileDirectory = Path.Combine(ArchitectureOutputPath, NetToAnyCodeGenerator<YamlCodeWriter>.GetComponentsDirectory(softwareSystemName, containerName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                return this;
            }

            var sourceCode = NetToAnyCodeGenerator<YamlCodeWriter>.GetComponentCode(
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

        public override NetToYamlArchitectureAsCodeWriter AddComponentInterface(
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

            var fileDirectory = Path.Combine(ArchitectureOutputPath, NetToAnyCodeGenerator<YamlCodeWriter>.GetComponentInterfacesDirectory(softwareSystemName, containerName, componentName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                return this;
            }

            var sourceCode = NetToAnyCodeGenerator<YamlCodeWriter>.GetComponentInterfaceCode(
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

        public void AddFlowToComponentInterfaceClass(string filePath,
            IEnumerable<NetToAnyMethodTriggerMapper>? methodTriggerMappers = null,
            IEnumerable<NetToAnyAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            var systemMethodDeclaration = ComponentMethodInterfaceObjectMap.GetValueOrDefault(filePath);
            if (systemMethodDeclaration == null)
            {
                return;
            }

            var architectureObject = GetJsonObjectFromFile(filePath); ;
            var flowCode = NetToAnyCodeGenerator<YamlCodeWriter>.GetFlowCode(
                systemMethodDeclaration,
                new JObjectArchitectureAsCodeReaderStrategy(architectureObject),
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

        public List<string> WithComponentInterfaces()
        {
            string pattern = @"^.*\\SoftwareSystems\\.*\\Containers\\.*\\Components\\.*\\Interfaces\\.*\.yaml$";
            List<string> result = Directory.GetFiles(ArchitectureOutputPath, "*.yaml", SearchOption.AllDirectories)
                .Where(x => Regex.IsMatch(x, pattern)).ToList();

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

        public override string? GetComponentInterfaceAlias(string filePathPattern)
        {
            var result = string.Empty;

            var filePath = Directory.GetFiles(ArchitectureOutputPath, "*.yaml", SearchOption.AllDirectories)
                .FirstOrDefault(x => Regex.IsMatch(x, filePathPattern));

            if(!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    var jsonObject = GetJsonObjectFromFile(filePath);
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
