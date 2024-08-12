using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using C4InterFlow.Structures;
using System.Text;
using System.Text.RegularExpressions;
using Serilog;

namespace C4InterFlow.Automation.Writers
{
    public class CSharpToCSharpAaCWriter : CSharpToAnyAaCWriter
    {
        private string FileExtension => "cs";
        public CSharpToCSharpAaCWriter(string softwareSystemSolutionPath, string architectureRootNamespace) : base(softwareSystemSolutionPath, architectureRootNamespace)
        {
        }

        public MSBuildWorkspace? ArchitectureWorkspace { get; private set; }
        public CSharpToCSharpAaCWriter WithArchitectureProject(string architectureProjectPath)
        {
            ArchitectureWorkspace = MSBuildWorkspace.Create(new Dictionary<string, string>()
                {
                    { "BuildingInsideVisualStudio", "true" }
                });

            ArchitectureWorkspace.OpenProjectAsync(architectureProjectPath).Wait();
            return this;
        }
        public override CSharpToCSharpAaCWriter AddSoftwareSystem(string name, string? boundary = null, string? label = null, string? description = null)
        {
            var project = ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == ArchitectureNamespace);

            if (project == null)
            {
                Log.Warning("Project {Name} was not found in {Solution} solution", ArchitectureNamespace, ArchitectureWorkspace.CurrentSolution.FilePath);
                return this;
            }

            var documentName = $"{name}.{FileExtension}";

            var projectDirectory = project.FilePath.Replace($"{project.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetSoftwareSystemsDirectory());
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (project.Documents.Any(x => x.FilePath == filePath))
            {
                Log.Warning("Document {Name} already exists in {Project} project", filePath, project.Name);
                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetSoftwareSystemCode(
                ArchitectureNamespace,
                name,
                CSharpCodeWriter.GetLabel(name),
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

        public override CSharpToCSharpAaCWriter AddContainer(string softwareSystemName, string name, string? containerType = null, string? label = null, string? description = null)
        {
            var project = ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == ArchitectureNamespace);

            if (project == null)
            {
                Log.Warning("Project {Name} was not found in {Solution} solution", ArchitectureNamespace, ArchitectureWorkspace.CurrentSolution.FilePath);
                return this;
            }

            var documentName = $"{name}.{FileExtension}";

            var projectDirectory = project.FilePath.Replace($"{project.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetContainersDirectory(softwareSystemName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (project.Documents.Any(x => x.FilePath == filePath))
            {
                Log.Warning("Document {Name} already exists in {Project} project", filePath, project.Name);
                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetContainerCode(
                ArchitectureNamespace,
                softwareSystemName,
                name,
                CSharpCodeWriter.GetLabel(name),
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

        public override CSharpToCSharpAaCWriter AddComponent(string softwareSystemName, string containerName, string name, ComponentType componentType = ComponentType.None)
        {
            var project = ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == ArchitectureNamespace);

            if (project == null)
            {
              
                Log.Warning("Project {Name} was not found in {Solution} solution", ArchitectureNamespace, ArchitectureWorkspace.CurrentSolution.FilePath);
                return this;
            }

            var documentName = $"{name}.{FileExtension}";

            var projectDirectory = project.FilePath.Replace($"{project.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetComponentsDirectory(softwareSystemName, containerName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (project.Documents.Any(x => x.FilePath == filePath))
            {
                Log.Warning("Document {Name} already exists in {Project} project", filePath, project.Name);
                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetComponentCode(
                ArchitectureNamespace,
                softwareSystemName,
                containerName,
                name,
                CSharpCodeWriter.GetLabel(name),
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

        public override CSharpToCSharpAaCWriter AddComponentInterface(
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
            var architectureProject = ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == ArchitectureNamespace);

            if (architectureProject == null)
            {
                Log.Warning("Project {Name} was not found in {Solution} solution", ArchitectureNamespace, ArchitectureWorkspace.CurrentSolution.FilePath);
                return this;
            }

            var documentName = $"{name}.{FileExtension}";

            var projectDirectory = architectureProject.FilePath.Replace($"{architectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetComponentInterfacesDirectory(softwareSystemName, containerName, componentName));
            var filePath = Path.Combine(fileDirectory, documentName);

            Directory.CreateDirectory(fileDirectory);

            if (architectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Log.Warning("Document {Name} already exists in {Project} project", filePath, architectureProject.Name);
                return this;
            }

            var sourceCode = CSharpToAnyCodeGenerator<CSharpCodeWriter>.GetComponentInterfaceCode(
                architectureNamespace: ArchitectureNamespace,
                softwareSystemName: softwareSystemName,
                containerName: containerName,
                componentName: componentName,
                name: name,
                label: label ?? CSharpCodeWriter.GetLabel(name),
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

        public IEnumerable<ClassDeclarationSyntax> WithComponentInterfaces(bool reloadArchitecture = false)
        {
            var result = new List<ClassDeclarationSyntax>();

            var project = ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == ArchitectureNamespace);
            if (reloadArchitecture)
            {
                ArchitectureWorkspace.CloseSolution();
                project = ArchitectureWorkspace.OpenProjectAsync(project.FilePath).Result;
            }

            var interfaceInstanceType = typeof(Structures.Interfaces.IInterfaceInstance);
            string pattern = @"^.*\\Components\\.*\\Interfaces\\.*$";

            var compilation = project.GetCompilationAsync().Result;

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

        public ClassDeclarationSyntax? WithComponentInterface(string fileNamePattern)
        {
            var project = ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == ArchitectureNamespace);

            var interfaceInstanceType = typeof(Structures.Interfaces.IInterfaceInstance);

            var compilation = project.GetCompilationAsync().Result;
            var syntaxTree = compilation.SyntaxTrees.Where(x => Regex.IsMatch(x.FilePath, fileNamePattern)).FirstOrDefault();

            if (syntaxTree != null)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                var root = syntaxTree.GetRoot();

                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Where(c => c.BaseList != null &&
                        syntaxTree.FilePath.EndsWith($"{c.Identifier.ValueText}.{FileExtension}") &&
                        c.BaseList.Types.Any(t => t.Type.ToString() == interfaceInstanceType.Name));

                return classes.FirstOrDefault();
            }




            return null;
        }

        public override string? GetComponentInterfaceAlias(string filePathPattern)
        {
            return WithComponentInterface(filePathPattern)?.GetAliasFieldValue();
        }

        public override string GetFileExtension()
        {
            return FileExtension;
        }
    }
}
