using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using C4InterFlow.Elements;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

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
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

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
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

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
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

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
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

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

        public void AddFlowToComponentInterfaceClass(string path,
            IEnumerable<NetToAnyMethodTriggerMapper>? methodTriggerMappers = null,
            IEnumerable<NetToAnyAlternativeInvocationMapperConfig>? alternativeInvocationMappers = null)
        {
            var systemMethodDeclaration = ComponentMethodInterfaceObjectMap.GetValueOrDefault(path);

            if (systemMethodDeclaration == null) return;

            var architectureObject = new JObject();
            var flowCode = NetToAnyCodeGenerator<YamlCodeWriter>.GetFlowCode(
                systemMethodDeclaration,
                new JObjectArchitectureAsCodeContext(architectureObject),
                this,
                alternativeInvocationMappers);
        }

        public List<string> WithComponentInterfaces()
        {
            string pattern = @"^.*\\SoftwareSystems\\.*\\Containers\\.*\\Components\\.*\\Interfaces\\.*\.yaml$";
            List<string> result = Directory.GetFiles(ArchitectureOutputPath, "*.yaml", SearchOption.AllDirectories)
                .Where(x => Regex.IsMatch(x, pattern)).ToList();

            return result;
        }

        public string? WithComponentInterface(string pattern)
        {
            string result = Directory.GetFiles(ArchitectureOutputPath, "*.yaml", SearchOption.AllDirectories)
                .FirstOrDefault(x => Regex.IsMatch(x, pattern));

            return result;
        }

    }
}
