using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using System.Text;
using CsvHelper;
using System.Globalization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace C4InterFlow.Automation
{
    public class CsvToNetArchitectureAsCodeWriter : CsvToAnyArchitectureAsCodeWriter
    {
        public Project? ArchitectureProject { get; private set; }
        public MSBuildWorkspace? ArchitectureWorkspace { get; private set; }

        protected CsvToNetArchitectureAsCodeWriter(string architectureInputPath)
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

        public static CsvToNetArchitectureAsCodeWriter WithCsvData(string csvRootPath)
        {
            return new CsvToNetArchitectureAsCodeWriter(csvRootPath);
        }
        public CsvToNetArchitectureAsCodeWriter WithArchitectureRootNamespace(string architectureRootNamespace)
        {
            ArchitectureNamespace = architectureRootNamespace.Trim();
            return this;
        }

        public CsvToNetArchitectureAsCodeWriter WithArchitectureProject(string architectureProjectPath)
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

        public CsvToNetArchitectureAsCodeWriter AddActorClass(string actorName, string type)
        {
            var documentName = $"{actorName}.cs";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, NetToAnyCodeGenerator<NetCodeWriter>.GetActorsDirectory());
            var filePath = Path.Combine(fileDirectory, documentName);

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{ArchitectureProject.Name}' Project.");
                return this;
            }

            var sourceCode = NetToAnyCodeGenerator<NetCodeWriter>.GetActorCode(
                ArchitectureNamespace,
                type,
                actorName,
                NetCodeWriter.GetLabel(actorName));

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

        public CsvToNetArchitectureAsCodeWriter AddBusinessProcessClass(string businessProcessName, BusinessActivity[] businessActivities)
        {
            var documentName = $"{businessProcessName}.cs";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, NetToAnyCodeGenerator<NetCodeWriter>.GetBusinessProcessesDirectory());
            var filePath = Path.Combine(fileDirectory, documentName);

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{ArchitectureProject.Name}' Project.");
                return this;
            }

            var businessActivitiesSourceCode = new StringBuilder();
            foreach (var businessActivity in businessActivities
                .Where(x => !string.IsNullOrEmpty(x.UsesSoftwareSystemInterfaceAlias) || 
                    !string.IsNullOrEmpty(x.UsesContainerInterfaceAlias))
                .GroupBy(x => new { x.Name, x.ActorAlias })
                .Select(g => new
                {
                    g.Key.Name,
                    g.Key.ActorAlias,
                    Uses = g.Select(x => $"{ArchitectureNamespace}.SoftwareSystems.{(string.IsNullOrEmpty(x.UsesContainerInterfaceAlias) ? x.UsesSoftwareSystemInterfaceAlias : x.UsesContainerInterfaceAlias)}").ToArray()
                }))
            {
                businessActivitiesSourceCode.Append(NetToAnyCodeGenerator<NetCodeWriter>.GetBusinessActivityCode(
                    businessActivity.Name,
                    $"{ArchitectureNamespace}.Actors.{businessActivity.ActorAlias}",
                    businessActivity.Uses));
            }

            var sourceCode = NetToAnyCodeGenerator<NetCodeWriter>.GetBusinessProcessCode(
                ArchitectureNamespace,
                businessProcessName,
                NetCodeWriter.GetLabel(businessProcessName),
                businessActivitiesSourceCode.ToString());

            var tree = CSharpSyntaxTree.ParseText(sourceCode.ToString());

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, tree.GetRoot().ToFullString());
            }

            return this;
        }

        public CsvToNetArchitectureAsCodeWriter AddSoftwareSystemClass(string softwareSystemName, string? boundary = null)
        {
            var documentName = $"{softwareSystemName}.cs";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, NetToAnyCodeGenerator<NetCodeWriter>.GetSoftwareSystemsDirectory());
            var filePath = Path.Combine(fileDirectory, documentName);

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{ArchitectureProject.Name}' Project.");
                return this;
            }

            var sourceCode = NetToAnyCodeGenerator<NetCodeWriter>.GetSoftwareSystemCode(
                ArchitectureNamespace,
                softwareSystemName,
                NetCodeWriter.GetLabel(softwareSystemName),
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
        public CsvToNetArchitectureAsCodeWriter AddSoftwareSystemInterfaceClass(SoftwareSystemInterface softwareSystemInterface)
        {
            var softwareSystemName = softwareSystemInterface.SoftwareSystemAlias;
            var interfaceName = softwareSystemInterface.Alias.Split('.').Last();
            var documentName = $"{interfaceName}.cs";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, NetToAnyCodeGenerator<NetCodeWriter>.GetSoftwareSystemInterfacesDirectory(softwareSystemName));
            
            Directory.CreateDirectory(fileDirectory);
            var filePath = Path.Combine(fileDirectory, documentName);

            if (!SoftwareSystemInterfaceClassFileNameMap.Keys.Contains(filePath))
            {
                SoftwareSystemInterfaceClassFileNameMap.Add(filePath, softwareSystemInterface);
            }

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{ArchitectureProject.Name}' Project.");
                return this;
            }

            var sourceCode = NetToAnyCodeGenerator<NetCodeWriter>.GetSoftwareSystemInterfaceCode(
                ArchitectureNamespace,
                softwareSystemName,
                interfaceName,
                NetCodeWriter.GetLabel(interfaceName));

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

        public CsvToNetArchitectureAsCodeWriter AddContainerClass(string softwareSystemName, string containerName, string? containerType = null )
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

            var sourceCode = NetToAnyCodeGenerator<NetCodeWriter>.GetContainerCode(
                ArchitectureNamespace,
                softwareSystemName,
                containerName,
                NetCodeWriter.GetLabel(containerName),
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

        public CsvToNetArchitectureAsCodeWriter AddContainerInterfaceClass(ContainerInterface containerInterface)
        {
            var containerAliasSegments = containerInterface.ContainerAlias.Split('.');
            var softwareSystemName = containerAliasSegments[Array.IndexOf(containerAliasSegments, "Containers") - 1];
            var containerName = containerAliasSegments.Last();
            var interfaceName = containerInterface.Alias.Split('.').Last();
            var documentName = $"{interfaceName}.cs";
            var projectDirectory = ArchitectureProject.FilePath.Replace($"{ArchitectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, NetToAnyCodeGenerator<NetCodeWriter>.GetContainerInterfaceDirectory(softwareSystemName, containerName));
            Directory.CreateDirectory(fileDirectory);
            var filePath = Path.Combine(fileDirectory, documentName);

            if (!ContainerInterfaceClassFileNameMap.Keys.Contains(filePath))
            {
                ContainerInterfaceClassFileNameMap.Add(filePath, containerInterface);
            }

            if (ArchitectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{ArchitectureProject.Name}' Project.");
                return this;
            }

            var sourceCode = NetToAnyCodeGenerator<NetCodeWriter>.GetContainerInterfaceCode(
                ArchitectureNamespace,
                softwareSystemName,
                containerName,
                interfaceName,
                NetCodeWriter.GetLabel(interfaceName));

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
