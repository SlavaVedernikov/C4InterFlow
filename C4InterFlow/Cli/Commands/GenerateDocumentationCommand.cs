using System.CommandLine;
using C4InterFlow.Structures;
using C4InterFlow.Cli.Commands.Options;
using C4InterFlow.Automation;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fluid;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace C4InterFlow.Cli.Commands;

public class GenerateDocumentationCommand : Command
{
    private const string COMMAND_NAME = "generate-documentation";
    public GenerateDocumentationCommand() : base(COMMAND_NAME,
        "Generate Documentation")
    {
        var structuresQueryOption = StructuresQueryOption.Get();
        var documentationTemplateFileOption = DocumentationTemplateFileOption.Get();
        var outputDirectoryOption = OutputDirectoryOption.Get();
        var fileExtensionOption = FileExtensionOption.Get();
        var architectureAsCodeInputPathsOption = AaCInputPathsOption.Get();
        var architectureAsCodeReaderStrategyTypeOption = AaCReaderStrategyTypeOption.Get();

        AddOption(structuresQueryOption);
        AddOption(documentationTemplateFileOption);
        AddOption(outputDirectoryOption);
        AddOption(fileExtensionOption);
        AddOption(architectureAsCodeInputPathsOption);
        AddOption(architectureAsCodeReaderStrategyTypeOption);

        this.SetHandler(async (structuresQuery, documentationTemplateFile, outputDirectory, fileExtension, architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType) =>
            {
                await Execute(structuresQuery, documentationTemplateFile, outputDirectory, fileExtension, architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType);
            },
            structuresQueryOption, documentationTemplateFileOption, outputDirectoryOption, fileExtensionOption, architectureAsCodeInputPathsOption, architectureAsCodeReaderStrategyTypeOption);
    }

    private static async Task<int> Execute(string structuresQuery, string documentationTemplateFile, string outputDirectory, string fileExtension, string[] architectureAsCodeInputPaths, string architectureAsCodeReaderStrategyType)
    {
        try
        {
            Console.WriteLine($"'{COMMAND_NAME}' command is executing...");

            if (!AaCReaderContext.HasStrategy)
            {
                Utils.SetArchitectureAsCodeReaderContext(architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType);
            }

            string templateSource = File.ReadAllText(documentationTemplateFile);

            var structureAliases = Utils.ResolveStructures(new[] { structuresQuery });
            var containersKey = "Containers";
            var interfacesKey = "Interfaces";
            var attributesKey = "Attributes";

            foreach (var structureAlias in structureAliases)
            {
                var structure = C4InterFlow.Utils.GetInstance<Structure>(structureAlias);
                if (structure != null)
                {
                    var currentStructureObject = new Dictionary<string, object>();
                    AddStructureFields(currentStructureObject, structure);

                    if (structure is SoftwareSystem)
                    {
                        var softwareSystem = (SoftwareSystem)structure;
                        currentStructureObject["Boundary"] = softwareSystem.Boundary.ToString();

                        var containerAliases = Utils.ResolveStructures(new[] { $"{structure.Alias}.{containersKey}.*" });

                        if (containerAliases.Any())
                        {
                            currentStructureObject[containersKey] = new Dictionary<string, object>();
                            var containersObject = (Dictionary<string, object>)currentStructureObject[containersKey];

                            foreach (var containerAlias in containerAliases)
                            {
                                var container = C4InterFlow.Utils.GetInstance<Structures.Container>(containerAlias);
                                if (container != null)
                                {
                                    var containerDictionary = new Dictionary<string, object>();
                                    AddStructureFields(containerDictionary, container);
                                    containerDictionary["ContainerType"] = container.ContainerType.ToString();
                                    containersObject[container.Name] = containerDictionary;
                                }
                            }
                        }

                        var interfaceAliases = Utils.ResolveStructures(new[] { $"{structure.Alias}.{interfacesKey}.*" });

                        if (interfaceAliases.Any())
                        {
                            currentStructureObject[interfacesKey] = new Dictionary<string, object>();
                            var interfacesObject = (Dictionary<string, object>)currentStructureObject[interfacesKey];

                            foreach (var interfaceAlias in interfaceAliases)
                            {
                                var @interface = C4InterFlow.Utils.GetInstance<Interface>(interfaceAlias);
                                if (@interface != null)
                                {
                                    var interfaceDicionary = new Dictionary<string, object>();
                                    AddStructureFields(interfaceDicionary, @interface);
                                    interfacesObject[@interface.Name] = interfaceDicionary;
                                }
                            }
                        }

                        var attributeAliases = Utils.ResolveStructures(new[] { $"{structure.Alias}.{attributesKey}.*" });

                        if (attributeAliases.Any())
                        {
                            currentStructureObject[attributesKey] = new Dictionary<string, object>();
                            var attributesObject = (Dictionary<string, object>)currentStructureObject[attributesKey];

                            foreach (var attributeAlias in attributeAliases)
                            {
                                var attribute = C4InterFlow.Utils.GetInstance<StructureAttribute>(attributeAlias);
                                if (attribute != null)
                                {
                                    var attributeDicionary = new Dictionary<string, object>();
                                    AddStructureFields(attributeDicionary, attribute);

                                    attributeDicionary["Value"] = attribute.Value;
                                    attributesObject[attribute.Name] = attributeDicionary;
                                }
                            }
                        }


                        var document = RenderDocument(currentStructureObject, templateSource);
                        var directoryPath = Path.Join(outputDirectory, "Software Systems", structure.Label);
                        Directory.CreateDirectory(directoryPath);
                        var filePath = Path.Join(directoryPath, $"About.{fileExtension}");
                        File.WriteAllText(filePath, document);
                    }
                }
            }

            Console.WriteLine($"{COMMAND_NAME} command completed.");
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Documentation generation failed with exception(s) '{e.Message}'{(e.InnerException != null ? $", '{e.InnerException}'" : string.Empty)}.");
            return 1;
        }
    }

    private static string RenderDocument(Dictionary<string, object> data, string templateSource)
    {
        var result = string.Empty;
        var parser = new FluidParser();
        if (parser.TryParse(templateSource, out var template, out var error))
        {
            var context = new TemplateContext(data);

            result = template.Render(context);
        }
        else
        {
            Console.WriteLine($"Error rendering template: {error}");
        }

        return result;
    }

    private static void AddStructureFields(Dictionary<string, object> rootObject, Structure structure)
    {
        rootObject["Name"] = structure.Name;
        rootObject["Alias"] = structure.Alias;
        rootObject["Lable"] = structure.Label;
        rootObject["Description"] = structure.Description;
    }

    private static Dictionary<string, object> GetNamespaceObject(Dictionary<string, object> architectureObject, string alias)
    {
        var result = new Dictionary<string, object>();

        if (architectureObject == null) return result;

        Match match = Regex.Match(alias, @"(.*)(?=\.SoftwareSystems)");

        if (match.Success)
        {
            result = architectureObject;
            var namespaceSegments = match.Value.Split('.');
            
            foreach (var segment in namespaceSegments)
            {
                if (!result.ContainsKey(segment))
                {
                    result[segment] = new Dictionary<string, object>();
                }

                result = (Dictionary<string, object>)result[segment];
            }
        }

        return result;
    }
    
}
