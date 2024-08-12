using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Serilog;

namespace C4InterFlow.Automation.Writers
{
    public class JsonToCSharpAaCWriter : JsonToAnyAaCWriter
    {
        private string FileExtension => "cs";
        public Project? ArchitectureProject { get; private set; }
        public MSBuildWorkspace? ArchitectureWorkspace { get; private set; }

        protected JsonToCSharpAaCWriter(string architectureInputPath) : base(architectureInputPath)
        {
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

        public static JsonToCSharpAaCWriter WithJsonData(string jsonRootPath)
        {
            return new JsonToCSharpAaCWriter(jsonRootPath);
        }

        public JsonToCSharpAaCWriter WithArchitectureRootNamespace(string architectureRootNamespace)
        {
            ArchitectureNamespace = architectureRootNamespace.Trim();
            return this;
        }

        public JsonToCSharpAaCWriter WithArchitectureProject(string architectureProjectPath)
        {
            ArchitectureProject = ArchitectureWorkspace.OpenProjectAsync(architectureProjectPath).Result;
            return this;
        }

        public override JsonToCSharpAaCWriter AddSoftwareSystem(string name, string? boundary = null, string? label = null, string? description = null)
        {
            var documentName = $"{name}.{FileExtension}";
            var fileDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Log.Warning("Document {Name} already exists in {Project} project", filePath, ArchitectureProject.Name);

                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetSoftwareSystemCode(
                ArchitectureNamespace,
                name,
                label ?? CSharpCodeWriter.GetLabel(name),
                description);

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
        public override JsonToCSharpAaCWriter AddSoftwareSystemInterface(
            string softwareSystemName,
            string name,
            string? label = null,
            string? description = null,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null)
        {
            var documentName = $"{name}.{FileExtension}";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetSoftwareSystemInterfacesDirectory(softwareSystemName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (!SoftwareSystemInterfaceAaCPathToJObjectMap.ContainsKey(filePath))
            {
                var softwareSystemInterface = GetSoftwareSystemInterface(softwareSystemName, name);
                if (softwareSystemInterface != null)
                {
                    SoftwareSystemInterfaceAaCPathToJObjectMap.Add(filePath, softwareSystemInterface);
                }
            }


            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Log.Warning("Document {Name} already exists in {Project} project", filePath, ArchitectureProject.Name);

                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetSoftwareSystemInterfaceCode(
                ArchitectureNamespace,
                softwareSystemName,
                name,
                label ?? CSharpCodeWriter.GetLabel(name),
                description,
                protocol);

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

        public override JsonToCSharpAaCWriter AddContainer(string softwareSystemName, string name, string? containerType = null, string? label = null, string? description = null)
        {
            var documentName = $"{name}.{FileExtension}";

            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetContainersDirectory(softwareSystemName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Log.Warning("Document {Name} already exists in {Project} project", filePath, ArchitectureProject.Name);

                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetContainerCode(
                ArchitectureNamespace,
                softwareSystemName,
                name,
                label ?? CSharpCodeWriter.GetLabel(name),
                containerType,
                description);

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

        public override JsonToCSharpAaCWriter AddContainerInterface(
            string softwareSystemName,
            string containerName,
            string name,
            string? label = null,
            string? description = null,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null)
        {
            var documentName = $"{name}.{FileExtension}";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetContainerInterfaceDirectory(softwareSystemName, containerName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (!ContainerInterfaceAaCPathToJObjectMap.ContainsKey(filePath))
            {
                var containerInterface = GetContainerInterface(softwareSystemName, containerName, name);
                if(containerInterface != null)
                {
                    ContainerInterfaceAaCPathToJObjectMap.Add(filePath, containerInterface);
                }
            }

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Log.Warning("Document {Name} already exists in {Project} project", filePath, ArchitectureProject.Name);

                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetContainerInterfaceCode(
                ArchitectureNamespace,
                softwareSystemName,
                containerName,
                name,
                label ?? CSharpCodeWriter.GetLabel(name),
                description,
                protocol);

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

        public override string GetFileExtension()
        {
            return FileExtension;
        }

        public IEnumerable<ClassDeclarationSyntax> WithSoftwareSystemInterfaceClasses(string softwareSystemName, bool reloadArchitectureProject = false)
        {
            var result = new List<ClassDeclarationSyntax>();

            if (reloadArchitectureProject)
            {
                ArchitectureWorkspace.CloseSolution();
                ArchitectureProject = ArchitectureWorkspace.OpenProjectAsync(ArchitectureProject.FilePath).Result;
            }

            var interfaceInstanceType = typeof(Structures.Interfaces.IInterfaceInstance);
            string pattern = @$"^.*\\{softwareSystemName}\\Interfaces\\.*$";

            var compilation = ArchitectureProject.GetCompilationAsync().Result;

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                if (!Regex.IsMatch(syntaxTree.FilePath, pattern)) continue;

                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                var root = syntaxTree.GetRoot();

                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Where(c => c.BaseList != null &&
                        syntaxTree.FilePath.EndsWith($"{c.Identifier.ValueText}.{FileExtension}") &&
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

            var interfaceInstanceType = typeof(Structures.Interfaces.IInterfaceInstance);
            string pattern = @$"^.*\\Containers\\{containerName ?? ".*"}\\Interfaces\\.*$";

            var compilation = ArchitectureProject.GetCompilationAsync().Result;

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                if (!Regex.IsMatch(syntaxTree.FilePath, pattern)) continue;

                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                var root = syntaxTree.GetRoot();

                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Where(c => c.BaseList != null &&
                        syntaxTree.FilePath.EndsWith($"{c.Identifier.ValueText}.{FileExtension}") &&
                        c.BaseList.Types.Any(t => t.Type.ToString() == interfaceInstanceType.Name));

                result.AddRange(classes);
            }

            return result;
        }
    }
}
