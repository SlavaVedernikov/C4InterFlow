using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using C4InterFlow.Elements;
using System.Text;
using System.Text.RegularExpressions;

namespace C4InterFlow.Automation
{
    public class NetToYamlArchitectureAsCodeWriter : NetToAnyArchitectureAsCodeWriter<ClassDeclarationSyntax, Document>
    {
        protected string? ArchitectureOutputPath { get; private set; }
        public NetToYamlArchitectureAsCodeWriter(string softwareSystemSolutionPath, string architectureRootNamespace, string architectureOutputPath) : base(softwareSystemSolutionPath, architectureRootNamespace)
        {
            ArchitectureOutputPath = architectureOutputPath;
        }

        public override NetToYamlArchitectureAsCodeWriter AddSoftwareSystem(string softwareSystemName)
        {
            var documentName = $"{softwareSystemName}.yaml";
            var fileDirectory = Path.Combine(ArchitectureOutputPath, NetToAnyCodeGenerator<NetCodeWriter>.GetSoftwareSystemsDirectory());
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                return this;
            }

            var sourceCode = NetToAnyCodeGenerator<NetCodeWriter>.GetSoftwareSystemCode(
                ArchitectureNamespace,
                softwareSystemName,
                NetCodeWriter.GetLabel(softwareSystemName));


            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, sourceCode.ToString());
            }

            return this;
        }

        public override NetToYamlArchitectureAsCodeWriter AddContainer(string softwareSystemName, string containerName)
        {
            var documentName = $"{containerName}.yaml";

            var fileDirectory = Path.Combine(ArchitectureOutputPath, NetToAnyCodeGenerator<NetCodeWriter>.GetContainersDirectory(softwareSystemName));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                return this;
            }

            var sourceCode = NetToAnyCodeGenerator<NetCodeWriter>.GetContainerCode(
                ArchitectureNamespace,
                softwareSystemName,
                containerName,
                NetCodeWriter.GetLabel(containerName));

            var tree = CSharpSyntaxTree.ParseText(sourceCode.ToString());
            var root = tree.GetRoot();
            var formattedRoot = root.NormalizeWhitespace();
            var formattedSourceCode = formattedRoot.ToFullString();

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, formattedSourceCode);
            }

            return this;
        }

        public override NetToYamlArchitectureAsCodeWriter AddComponent(string softwareSystemName, string containerName, string componentName, ComponentType componentType = ComponentType.None)
        {
            var documentName = $"{componentName}.cs";

            var fileDirectory = Path.Combine(ArchitectureOutputPath, NetToAnyCodeGenerator<NetCodeWriter>.GetComponentsDirectory(softwareSystemName, containerName));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                return this;
            }

            var sourceCode = NetToAnyCodeGenerator<NetCodeWriter>.GetComponentCode(
                ArchitectureNamespace,
                softwareSystemName,
                containerName,
                componentName,
                NetCodeWriter.GetLabel(componentName),
                componentType.ToString());

            var tree = CSharpSyntaxTree.ParseText(sourceCode.ToString());
            var root = tree.GetRoot();
            var formattedRoot = root.NormalizeWhitespace();
            var formattedSourceCode = formattedRoot.ToFullString();

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, formattedSourceCode);
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
            var documentName = $"{interfaceName}.cs";

            var fileDirectory = Path.Combine(ArchitectureOutputPath, NetToAnyCodeGenerator<NetCodeWriter>.GetComponentInterfacesDirectory(softwareSystemName, containerName, componentName));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (File.Exists(filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists.");
                return this;
            }

            var sourceCode = NetToAnyCodeGenerator<NetCodeWriter>.GetComponentInterfaceCode(
                architectureNamespace: ArchitectureNamespace,
                softwareSystemName: softwareSystemName,
                containerName: containerName,
                componentName: componentName,
                name: interfaceName,
                label: NetCodeWriter.GetLabel(interfaceName),
                protocol: protocol,
                path: path,
                input: input,
                output: output);

            var tree = CSharpSyntaxTree.ParseText(sourceCode.ToString());
            var root = tree.GetRoot();
            var formattedRoot = root.NormalizeWhitespace();
            var formattedSourceCode = formattedRoot.ToFullString();

            //TODO: Add support for Interface method overloads
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, formattedSourceCode);
            }

            return this;
        }

        public override IEnumerable<ClassDeclarationSyntax> WithComponentInterfaces(bool reloadArchitecture = false)
        {
            var result = new List<ClassDeclarationSyntax>();

            

            return result;
        }

        public override ClassDeclarationSyntax? WithComponentInterface(string pattern)
        {
            return null;
        }

        public override IEnumerable<Document>? WithDocuments()
        {
            return CurrentProject?.Documents.Where(x => !x.FilePath.Contains(@"\obj\"));
        }


    }
}
