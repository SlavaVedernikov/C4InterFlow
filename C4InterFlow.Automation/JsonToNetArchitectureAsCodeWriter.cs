using C4InterFlow.Automation;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace C4InterFlow.Automation
{
    public class JsonToNetArchitectureAsCodeWriter
    {
        public Project? ArchitectureProject { get; private set; }
        protected string? ArchitectureInputPath { get; private set; }

        public IList<JObject> ArchitectureJsonData { get; private set; }
        public string ArchitectureNamespace { get; private set; }
        public MSBuildWorkspace? ArchitectureWorkspace { get; private set; }

        public Dictionary<string, JObject> SoftwareSystemInterfaceClassFileNameMap { get; private set; } = new Dictionary<string, JObject>();
        public Dictionary<string, JObject> ContainerInterfaceClassFileNameMap { get; private set; } = new Dictionary<string, JObject>();

        protected JsonToNetArchitectureAsCodeWriter(string architectureInputPath)
        {
            ArchitectureInputPath = architectureInputPath;

            ArchitectureJsonData = new List<JObject>();

            var jsonFiles = Directory.EnumerateFiles(ArchitectureInputPath, "*.json", SearchOption.AllDirectories);

            foreach (var jsonFile in jsonFiles)
            {
                var jsonText = File.ReadAllText(jsonFile);
                var jsonObject = JObject.Parse(jsonText);
                ArchitectureJsonData.Add(jsonObject);
            }

            Console.WriteLine($"Loading data from '{ArchitectureInputPath}'...");


            RegisterInstanceVisualStudioInstance();
            ArchitectureWorkspace = MSBuildWorkspace.Create(new Dictionary<string, string>()
            {
                { "BuildingInsideVisualStudio", "true" }
            });
        }

        private static void RegisterInstanceVisualStudioInstance()
        {
            MSBuildLocator.RegisterInstance(MSBuildLocator.QueryVisualStudioInstances().OrderByDescending(
            instance => instance.Version).First());
        }

        public static JsonToNetArchitectureAsCodeWriter WithJsonData(string jsonRootPath)
        {
            return new JsonToNetArchitectureAsCodeWriter(jsonRootPath);
        }
        public JsonToNetArchitectureAsCodeWriter WithArchitectureRootNamespace(string architectureRootNamespace)
        {
            ArchitectureNamespace = architectureRootNamespace.Trim();
            return this;
        }

        public JsonToNetArchitectureAsCodeWriter WithArchitectureProject(string architectureProjectPath)
        {
            ArchitectureProject = ArchitectureWorkspace.OpenProjectAsync(architectureProjectPath).Result;
            return this;
        }

        public IEnumerable<JObject> WithSoftwareSystems()
        {
            var result = new List<JObject>();

            foreach(var item in ArchitectureJsonData)
            {
                result.AddRange(item.SelectTokens($"{ArchitectureNamespace}.SoftwareSystems.*").Select(x => x as JObject));
            }
            return result;
        }

        public JsonToNetArchitectureAsCodeWriter AddSoftwareSystemClass(string softwareSystemName)
        {
            var documentName = $"{softwareSystemName}.cs";
            var fileDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);            
            var filePath = Path.Combine(fileDirectory, documentName);

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{ArchitectureProject.Name}' Project.");
                return this;
            }

            var sourceCode = new StringBuilder(NetToAnyCodeGenerator<NetCodeWriter>.GetSoftwareSystemsCodeHeader(ArchitectureNamespace));

            sourceCode.Append(
                NetToAnyCodeGenerator<NetCodeWriter>.GetSoftwareSystemCode(
                ArchitectureNamespace,
                softwareSystemName,
                NetCodeWriter.GetLabel(softwareSystemName)));

            sourceCode.AppendLine("}");

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
        public JsonToNetArchitectureAsCodeWriter AddSoftwareSystemInterfaceClass(JObject softwareSystemInterface)
        {
            var softwareSystemInterfaceAliasSegments = softwareSystemInterface.Path.Split('.');
            var softwareSystemName = softwareSystemInterfaceAliasSegments[softwareSystemInterfaceAliasSegments.Length - 3];
            var interfaceName = softwareSystemInterfaceAliasSegments.Last();
            var documentName = $"{interfaceName}.cs";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, NetToAnyCodeGenerator<NetCodeWriter>.GetSoftwareSystemInterfacesDirectory(softwareSystemName));
            
            Directory.CreateDirectory(fileDirectory);
            var filePath = Path.Combine(fileDirectory, documentName);

            
            if (!SoftwareSystemInterfaceClassFileNameMap.ContainsKey(filePath))
            {
                SoftwareSystemInterfaceClassFileNameMap.Add(filePath, softwareSystemInterface);
            }
            

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{ArchitectureProject.Name}' Project.");
                return this;
            }

            var sourceCode = new StringBuilder(NetToAnyCodeGenerator<NetCodeWriter>.GetSoftwareSystemsCodeHeader(ArchitectureNamespace));

            sourceCode.Append(
                NetToAnyCodeGenerator<NetCodeWriter>.GetSoftwareSystemInterfaceCode(
                ArchitectureNamespace,
                softwareSystemName,
                interfaceName,
                NetCodeWriter.GetLabel(interfaceName)));

            sourceCode.AppendLine("}");

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

        public JsonToNetArchitectureAsCodeWriter AddContainerClass(string softwareSystemName, string containerName, string? containerType = null )
        {
            var documentName = $"{containerName}.cs";

            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, NetToAnyCodeGenerator<NetCodeWriter>.GetContainersDirectory(softwareSystemName));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{ArchitectureProject.Name}' Project.");
                return this;
            }

            var sourceCode = new StringBuilder(NetToAnyCodeGenerator<NetCodeWriter>.GetSoftwareSystemsCodeHeader(ArchitectureNamespace));

            sourceCode.Append(
                NetToAnyCodeGenerator<NetCodeWriter>.GetContainerCode(
                ArchitectureNamespace,
                softwareSystemName,
                containerName,
                NetCodeWriter.GetLabel(containerName),
                containerType));

            sourceCode.AppendLine("}");

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

        public JsonToNetArchitectureAsCodeWriter AddContainerInterfaceClass(JObject containerInterface)
        {
            var containerInterfaceAliasSegments = containerInterface.Path.Split('.');
            var softwareSystemName = containerInterfaceAliasSegments[Array.IndexOf(containerInterfaceAliasSegments, "Containers") - 1];
            var containerName = containerInterfaceAliasSegments[Array.IndexOf(containerInterfaceAliasSegments, "Containers") + 1]; ;
            var interfaceName = containerInterfaceAliasSegments.Last();
            var documentName = $"{interfaceName}.cs";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, NetToAnyCodeGenerator<NetCodeWriter>.GetContainerInterfaceDirectory(softwareSystemName, containerName));
            Directory.CreateDirectory(fileDirectory);
            var filePath = Path.Combine(fileDirectory, documentName);

            if (!ContainerInterfaceClassFileNameMap.ContainsKey(filePath))
            {
                ContainerInterfaceClassFileNameMap.Add(filePath, containerInterface);
            }

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{ArchitectureProject.Name}' Project.");
                return this;
            }

            var sourceCode = new StringBuilder(NetToAnyCodeGenerator<NetCodeWriter>.GetSoftwareSystemsCodeHeader(ArchitectureNamespace));

            sourceCode.Append(
                NetToAnyCodeGenerator<NetCodeWriter>.GetContainerInterfaceCode(
                ArchitectureNamespace,
                softwareSystemName,
                containerName,
                interfaceName,
                NetCodeWriter.GetLabel(interfaceName)));

            sourceCode.AppendLine("}");

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

        public IEnumerable<ClassDeclarationSyntax> WithSoftwareSystemInterfaceClasses(string softwareSystemName, bool reloadArchitectureProject = false)
        {
            var result = new List<ClassDeclarationSyntax>();

            if (reloadArchitectureProject)
            {
                ArchitectureWorkspace.CloseSolution();
                ArchitectureProject = ArchitectureWorkspace.OpenProjectAsync(ArchitectureProject.FilePath).Result;
            }

            var interfaceInstanceType = typeof(C4InterFlow.Elements.Interfaces.IInterfaceInstance);
            string pattern = @$"^.*\\{softwareSystemName}\\Interfaces\\.*$";

            var compilation = ArchitectureProject.GetCompilationAsync().Result;

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                if (!Regex.IsMatch(syntaxTree.FilePath, pattern)) continue;

                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                var root = syntaxTree.GetRoot();

                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Where(c => c.BaseList != null &&
                        syntaxTree.FilePath.EndsWith($"{c.Identifier.ValueText}.cs") &&
                        c.BaseList.Types.Any(t => t.Type.ToString() == interfaceInstanceType.Name));

                result.AddRange(classes);
            }

            return result;
        }

        public IEnumerable<ClassDeclarationSyntax> WithContainerInterfaceClasses(string? containerName = null, bool reloadArchitectureProject = false)
        {
            var result = new List<ClassDeclarationSyntax>();

            if (reloadArchitectureProject)
            {
                ArchitectureWorkspace.CloseSolution();
                ArchitectureProject = ArchitectureWorkspace.OpenProjectAsync(ArchitectureProject.FilePath).Result;
            }

            var interfaceInstanceType = typeof(C4InterFlow.Elements.Interfaces.IInterfaceInstance);
            string pattern = @$"^.*\\Containers\\{containerName ?? ".*"}\\Interfaces\\.*$";

            var compilation = ArchitectureProject.GetCompilationAsync().Result;

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                if (!Regex.IsMatch(syntaxTree.FilePath, pattern)) continue;

                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                var root = syntaxTree.GetRoot();

                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Where(c => c.BaseList != null &&
                        syntaxTree.FilePath.EndsWith($"{c.Identifier.ValueText}.cs") &&
                        c.BaseList.Types.Any(t => t.Type.ToString() == interfaceInstanceType.Name));

                result.AddRange(classes);
            }

            return result;
        }
    }
}
