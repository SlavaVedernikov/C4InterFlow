using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using System.Text;
using CsvHelper;
using System.Globalization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;
using Serilog;

namespace C4InterFlow.Automation.Writers
{
    public class CsvToCSharpAaCWriter : CsvToAnyAaCWriter
    {
        private string FileExtension => "cs";
        public Project? ArchitectureProject { get; private set; }
        public MSBuildWorkspace? ArchitectureWorkspace { get; private set; }

        protected CsvToCSharpAaCWriter(string architectureInputPath):base(architectureInputPath)
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

        public static CsvToCSharpAaCWriter WithCsvData(string csvRootPath)
        {
            return new CsvToCSharpAaCWriter(csvRootPath);
        }
        public CsvToCSharpAaCWriter WithArchitectureRootNamespace(string architectureRootNamespace)
        {
            ArchitectureNamespace = architectureRootNamespace.Trim();
            return this;
        }

        public CsvToCSharpAaCWriter WithArchitectureProject(string architectureProjectPath)
        {
            ArchitectureProject = ArchitectureWorkspace.OpenProjectAsync(architectureProjectPath).Result;
            return this;
        }

        public IEnumerable<CsvDataProvider.SoftwareSystem> WithSoftwareSystems()
        {
            return DataProvider.SoftwareSystemRecords.Where(x => !string.IsNullOrEmpty(x.Alias.Trim()));
        }

        public IEnumerable<CsvDataProvider.Actor> WithActors()
        {
            return DataProvider.ActorRecords.Where(x => !string.IsNullOrEmpty(x.Alias.Trim()));
        }

        public IEnumerable<CsvDataProvider.BusinessProcess> WithBusinessProcesses()
        {
            return DataProvider.BusinessProcessRecords.Where(x => !string.IsNullOrEmpty(x.Alias.Trim()));
        }

        public override CsvToCSharpAaCWriter AddActor(string name, string type, string? label = null)
        {
            var documentName = $"{name}.{FileExtension}";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetActorsDirectory());
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Log.Warning("Document {Name} already exists in {Project} project", filePath, ArchitectureProject.Name);

                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetActorCode(
                ArchitectureNamespace,
                type,
                name,
                string.IsNullOrEmpty(label) ? AnyCodeWriter.GetLabel(name) : label);

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

        public override CsvToCSharpAaCWriter AddBusinessProcess(string name, string? label = null)
        {
            var businessProcess = DataProvider.BusinessProcessRecords.FirstOrDefault(x => x.Alias == name);

            if(businessProcess == null) return this;

            var documentName = $"{name}.{FileExtension}";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetBusinessProcessesDirectory());
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Log.Warning("Document {Name} already exists in {Project} project", filePath, ArchitectureProject.Name);

                return this;
            }

            var businessActivitiesSourceCode = new StringBuilder();
            foreach (var businessActivity in GetBusinessProcessActivities(businessProcess))
            {
                businessActivitiesSourceCode.Append(CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetActivityCode(
                    businessActivity.Label,
                    businessActivity.Actor,
                    businessActivity.Flow?.Flows?.ToArray() ?? new Structures.Flow[] { }));
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetBusinessProcessCode(
                ArchitectureNamespace,
                name,
                string.IsNullOrEmpty(label) ? AnyCodeWriter.GetLabel(name) : label,
                businessActivitiesSourceCode.ToString());

            var tree = CSharpSyntaxTree.ParseText(sourceCode.ToString());

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, tree.GetRoot().ToFullString());
            }

            return this;
        }

        public override CsvToCSharpAaCWriter AddSoftwareSystem(string name, string? boundary = null, string? label = null, string? description = null)
        {
            var documentName = $"{name}.{FileExtension}";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetSoftwareSystemsDirectory());
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
                string.IsNullOrEmpty(label) ? CSharpCodeWriter.GetLabel(name) : label,
                description,
                boundary);

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
        public override CsvToCSharpAaCWriter AddSoftwareSystemInterface(
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

            if (!SoftwareSystemInterfaceAaCPathToCsvRecordMap.Keys.Contains(filePath))
            {
                var softwareSystemInterface = DataProvider.SoftwareSystemInterfaceRecords.FirstOrDefault(x => x.Alias == $"{softwareSystemName}.Interfaces.{name}");
                if(softwareSystemInterface != null)
                {
                    SoftwareSystemInterfaceAaCPathToCsvRecordMap.Add(filePath, softwareSystemInterface);
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
                string.IsNullOrEmpty(label) ? CSharpCodeWriter.GetLabel(name) : label,
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

        public override CsvToCSharpAaCWriter AddContainer(string softwareSystemName, string name, string? containerType = null, string? label = null, string? description = null, string? technology = null)
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
                string.IsNullOrEmpty(label) ? CSharpCodeWriter.GetLabel(name) : label,
                containerType,
                description,
                technology);

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

        public override CsvToCSharpAaCWriter AddContainerInterface(
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

            if (!ContainerInterfaceAaCPathToCsvRecordMap.Keys.Contains(filePath))
            {
                var containerInterface = DataProvider.ContainerInterfaceRecords.FirstOrDefault(x => x.Alias == $"{softwareSystemName}.Containers.{containerName}.Interfaces.{name}");
                if (containerInterface != null)
                {
                    ContainerInterfaceAaCPathToCsvRecordMap.Add(filePath, containerInterface);
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
