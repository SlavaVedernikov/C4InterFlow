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
        public NetToYamlArchitectureAsCodeWriter(string softwareSystemSolutionPath, string architectureRootNamespace) : base(softwareSystemSolutionPath, architectureRootNamespace)
        {
        }

        public MSBuildWorkspace? ArchitectureWorkspace { get; private set; }
        public NetToYamlArchitectureAsCodeWriter WithArchitectureProject(string architectureProjectPath)
        {
                ArchitectureWorkspace = MSBuildWorkspace.Create(new Dictionary<string, string>()
                {
                    { "BuildingInsideVisualStudio", "true" }
                });

                ArchitectureWorkspace.OpenProjectAsync(architectureProjectPath).Wait();
                return this;
        }
        public override NetToYamlArchitectureAsCodeWriter AddSoftwareSystem(string softwareSystemName)
        {
            var project = ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == ArchitectureNamespace);

            if (project == null)
            {
                Console.WriteLine($"Project '{ArchitectureNamespace}' was not found in '{ArchitectureWorkspace.CurrentSolution.FilePath}' Solution.");
                return this;
            }

            var documentName = $"{softwareSystemName}.cs";

            var projectDirectory = project.FilePath.Replace($"{project.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CodeGenerator<NetCodeWriter>.GetSoftwareSystemsDirectory());
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (project.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{project.Name}' Project.");
                return this;
            }

            var sourceCode = new StringBuilder(CodeGenerator<NetCodeWriter>.GetSoftwareSystemsCodeHeader(ArchitectureNamespace));

            sourceCode.Append(
                CodeGenerator<NetCodeWriter>.GetSoftwareSystemCode(
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

        public override NetToYamlArchitectureAsCodeWriter AddContainer(string softwareSystemName, string containerName)
        {
            var project = ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == ArchitectureNamespace);

            if (project == null)
            {
                Console.WriteLine($"Project '{ArchitectureNamespace}' was not found in '{ArchitectureWorkspace.CurrentSolution.FilePath}' Solution.");
                return this;
            }

            var documentName = $"{containerName}.cs";

            var projectDirectory = project.FilePath.Replace($"{project.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CodeGenerator<NetCodeWriter>.GetContainersDirectory(softwareSystemName));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (project.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{project.Name}' Project.");
                return this;
            }

            var sourceCode = new StringBuilder(CodeGenerator<NetCodeWriter>.GetSoftwareSystemsCodeHeader(ArchitectureNamespace));

            sourceCode.Append(
                CodeGenerator<NetCodeWriter>.GetContainerCode(
                ArchitectureNamespace,
                softwareSystemName,
                containerName,
                NetCodeWriter.GetLabel(containerName)));

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

        public override NetToYamlArchitectureAsCodeWriter AddComponent(string softwareSystemName, string containerName, string componentName, ComponentType componentType = ComponentType.None)
        {
            var project = ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == ArchitectureNamespace);

            if (project == null)
            {
                Console.WriteLine($"Project '{ArchitectureNamespace}' was not found in '{ArchitectureWorkspace.CurrentSolution.FilePath}' Solution.");
                return this;
            }

            var documentName = $"{componentName}.cs";

            var projectDirectory = project.FilePath.Replace($"{project.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CodeGenerator<NetCodeWriter>.GetComponentsDirectory(softwareSystemName, containerName));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (project.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{project.Name}' Project.");
                return this;
            }

            var sourceCode = new StringBuilder(CodeGenerator<NetCodeWriter>.GetSoftwareSystemsCodeHeader(ArchitectureNamespace));

            sourceCode.Append(
                CodeGenerator<NetCodeWriter>.GetComponentCode(
                ArchitectureNamespace,
                softwareSystemName,
                containerName,
                componentName,
                NetCodeWriter.GetLabel(componentName),
                componentType.ToString()));

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
            var architectureProject = ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == ArchitectureNamespace);

            if (architectureProject == null)
            {
                Console.WriteLine($"Project '{ArchitectureNamespace}' was not found in '{ArchitectureWorkspace.CurrentSolution.FilePath}' Solution.");
                return this;
            }

            var documentName = $"{interfaceName}.cs";

            var projectDirectory = architectureProject.FilePath.Replace($"{architectureProject.Name}.csproj", string.Empty);
            var fileDirectory = Path.Combine(projectDirectory, CodeGenerator<NetCodeWriter>.GetComponentInterfacesDirectory(softwareSystemName, containerName, componentName));
            Directory.CreateDirectory(fileDirectory);

            var filePath = Path.Combine(fileDirectory, documentName);

            if (architectureProject.Documents.Any(x => x.FilePath == filePath))
            {
                Console.WriteLine($"Document '{filePath}' already exists in '{architectureProject.Name}' Project.");
                return this;
            }

            var sourceCode = new StringBuilder(CodeGenerator<NetCodeWriter>.GetSoftwareSystemsCodeHeader(ArchitectureNamespace));

            sourceCode.Append(
                CodeGenerator<NetCodeWriter>.GetComponentInterfaceCode(
                architectureNamespace: ArchitectureNamespace,
                softwareSystemName: softwareSystemName,
                containerName: containerName,
                componentName: componentName,
                name: interfaceName,
                label: NetCodeWriter.GetLabel(interfaceName),
                protocol: protocol,
                path: path,
                input: input,
                output: output));

            sourceCode.AppendLine("}");

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

            var project = ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == ArchitectureNamespace);
            if(reloadArchitecture)
            {
                ArchitectureWorkspace.CloseSolution();
                project = ArchitectureWorkspace.OpenProjectAsync(project.FilePath).Result;
            }

            var interfaceInstanceType = typeof(C4InterFlow.Elements.Interfaces.IInterfaceInstance);
            string pattern = @"^.*\\Components\\.*\\Interfaces\\.*$";
            
            var compilation = project.GetCompilationAsync().Result;

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

        public override ClassDeclarationSyntax? WithComponentInterface(string pattern)
        {
            var project = ArchitectureWorkspace.CurrentSolution.Projects.FirstOrDefault(x => x.Name == ArchitectureNamespace);

            var interfaceInstanceType = typeof(C4InterFlow.Elements.Interfaces.IInterfaceInstance);

            var compilation = project.GetCompilationAsync().Result;
            var syntaxTree = compilation.SyntaxTrees.Where(x => Regex.IsMatch(x.FilePath, pattern)).FirstOrDefault();

            if(syntaxTree != null)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                var root = syntaxTree.GetRoot();

                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Where(c => c.BaseList != null &&
                        syntaxTree.FilePath.EndsWith($"{c.Identifier.ValueText}.cs") &&
                        c.BaseList.Types.Any(t => t.Type.ToString() == interfaceInstanceType.Name));

                return classes.FirstOrDefault();
            }

                


            return null;
        }

        public override IEnumerable<Document>? WithDocuments()
        {
            return CurrentProject?.Documents.Where(x => !x.FilePath.Contains(@"\obj\"));
        }


    }
}
