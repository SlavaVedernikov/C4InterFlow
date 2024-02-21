using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using System.Text;
using CsvHelper;
using System.Globalization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace C4InterFlow.Automation.Writers
{
    public class CsvToCSharpAaCWriter : CsvToAnyAaCWriter
    {
        public Project? ArchitectureProject { get; private set; }
        public MSBuildWorkspace? ArchitectureWorkspace { get; private set; }

        protected CsvToCSharpAaCWriter(string architectureInputPath)
        {
            LoadData(architectureInputPath);

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

        public IEnumerable<SoftwareSystem> WithSoftwareSystems()
        {
            return SoftwareSystemRecords.Where(x => !string.IsNullOrEmpty(x.Alias.Trim()));
        }

        public IEnumerable<Actor> WithActors()
        {
            return ActorRecords.Where(x => !string.IsNullOrEmpty(x.Alias.Trim()));
        }

        public IEnumerable<BusinessProcess> WithBusinessProcesses()
        {
            return BusinessProcessRecords.Where(x => !string.IsNullOrEmpty(x.Alias.Trim()));
        }

        public CsvToCSharpAaCWriter AddActorClass(string name, string type, string? label = null)
        {
            var documentName = $"{name}.cs";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetActorsDirectory());
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{ArchitectureProject.Name}' Project.");
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

        public CsvToCSharpAaCWriter AddBusinessProcessClass(string name, Activity[] businessActivities, string? label = null)
        {
            var documentName = $"{name}.cs";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetBusinessProcessesDirectory());
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{ArchitectureProject.Name}' Project.");
                return this;
            }

            var businessActivitiesSourceCode = new StringBuilder();
            foreach (var businessActivity in businessActivities
                .Where(x => !string.IsNullOrEmpty(x.UsesSoftwareSystemInterface) ||
                    !string.IsNullOrEmpty(x.UsesContainerInterface))
                .GroupBy(x => new { x.Name, x.Actor })
                .Select(g => new
                {
                    g.Key.Name,
                    g.Key.Actor,
                    Uses = g.Select(x => $"{ArchitectureNamespace}.SoftwareSystems.{(string.IsNullOrEmpty(x.UsesContainerInterface) ? x.UsesSoftwareSystemInterface : x.UsesContainerInterface)}").ToArray()
                }))
            {
                businessActivitiesSourceCode.Append(CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetBusinessActivityCode(
                    businessActivity.Name,
                    $"{ArchitectureNamespace}.Actors.{businessActivity.Actor}",
                    businessActivity.Uses));
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

        public CsvToCSharpAaCWriter AddSoftwareSystemClass(string name, string? boundary = null, string? label = null)
        {
            var documentName = $"{name}.cs";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetSoftwareSystemsDirectory());
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{ArchitectureProject.Name}' Project.");
                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetSoftwareSystemCode(
                ArchitectureNamespace,
                name,
                string.IsNullOrEmpty(label) ? CSharpCodeWriter.GetLabel(name) : label,
                boundary: boundary);

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
        public CsvToCSharpAaCWriter AddSoftwareSystemInterfaceClass(SoftwareSystemInterface softwareSystemInterface)
        {
            var softwareSystemName = softwareSystemInterface.SoftwareSystem;
            var interfaceName = softwareSystemInterface.Alias.Split('.').Last();
            var documentName = $"{interfaceName}.cs";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetSoftwareSystemInterfacesDirectory(softwareSystemName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (!SoftwareSystemInterfaceClassFileNameMap.Keys.Contains(filePath))
            {
                SoftwareSystemInterfaceClassFileNameMap.Add(filePath, softwareSystemInterface);
            }

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{ArchitectureProject.Name}' Project.");
                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetSoftwareSystemInterfaceCode(
                ArchitectureNamespace,
                softwareSystemName,
                interfaceName,
                string.IsNullOrEmpty(softwareSystemInterface.Name) ? CSharpCodeWriter.GetLabel(interfaceName) : softwareSystemInterface.Name);

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

        public CsvToCSharpAaCWriter AddContainerClass(string softwareSystemName, string name, string? containerType = null, string? label = null)
        {
            var documentName = $"{name}.cs";

            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetContainersDirectory(softwareSystemName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);


            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{ArchitectureProject.Name}' Project.");
                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetContainerCode(
                ArchitectureNamespace,
                softwareSystemName,
                name,
                string.IsNullOrEmpty(label) ? CSharpCodeWriter.GetLabel(name) : label,
                containerType);

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

        public CsvToCSharpAaCWriter AddContainerInterfaceClass(ContainerInterface containerInterface)
        {
            var containerAliasSegments = containerInterface.Container.Split('.');
            var softwareSystemName = containerAliasSegments[Array.IndexOf(containerAliasSegments, "Containers") - 1];
            var containerName = containerAliasSegments.Last();
            var interfaceName = containerInterface.Alias.Split('.').Last();
            var documentName = $"{interfaceName}.cs";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetContainerInterfaceDirectory(softwareSystemName, containerName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (!ContainerInterfaceClassFileNameMap.Keys.Contains(filePath))
            {
                ContainerInterfaceClassFileNameMap.Add(filePath, containerInterface);
            }

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{ArchitectureProject.Name}' Project.");
                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetContainerInterfaceCode(
                ArchitectureNamespace,
                softwareSystemName,
                containerName,
                interfaceName,
                string.IsNullOrEmpty(containerInterface.Name) ? CSharpCodeWriter.GetLabel(interfaceName) : containerInterface.Name);

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
                        syntaxTree.FilePath.EndsWith($"{c.Identifier.ValueText}.cs") &&
                        c.BaseList.Types.Any(t => t.Type.ToString() == interfaceInstanceType.Name));

                result.AddRange(classes);
            }

            return result;
        }
    }
}
