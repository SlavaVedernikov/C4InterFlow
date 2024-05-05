using System.CommandLine;
using C4InterFlow.Structures;
using C4InterFlow.Cli.Commands.Options;
using C4InterFlow.Automation;
using System.Text.RegularExpressions;
using Fluid;

namespace C4InterFlow.Cli.Commands;

public class GenerateDocumentationCommand : Command
{
    private const string COMMAND_NAME = "generate-documentation";
    public GenerateDocumentationCommand() : base(COMMAND_NAME,
        "Generate Documentation")
    {
        var structuresOption = StructuresOption.Get();
        var templatesDirectoryOption = TemplatesDirectoryOption.Get();
        var outputDirectoryOption = OutputDirectoryOption.Get();
        var fileExtensionOption = FileExtensionOption.Get();
        var architectureAsCodeInputPathsOption = AaCInputPathsOption.Get();
        var architectureAsCodeReaderStrategyTypeOption = AaCReaderStrategyTypeOption.Get();

        AddOption(structuresOption);
        AddOption(templatesDirectoryOption);
        AddOption(outputDirectoryOption);
        AddOption(fileExtensionOption);
        AddOption(architectureAsCodeInputPathsOption);
        AddOption(architectureAsCodeReaderStrategyTypeOption);

        this.SetHandler(async (structures, templatesDirectory, outputDirectory, fileExtension, architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType) =>
            {
                await Execute(structures, templatesDirectory, outputDirectory, fileExtension, architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType);
            },
            structuresOption, templatesDirectoryOption, outputDirectoryOption, fileExtensionOption, architectureAsCodeInputPathsOption, architectureAsCodeReaderStrategyTypeOption);
    }

    private static async Task<int> Execute(string[] structures, string templatesDirectory, string outputDirectory, string fileExtension, string[] architectureAsCodeInputPaths, string architectureAsCodeReaderStrategyType)
    {
        try
        {
            Console.WriteLine($"'{COMMAND_NAME}' command is executing...");

            if (!AaCReaderContext.HasStrategy)
            {
                Utils.SetArchitectureAsCodeReaderContext(architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType);
            }

            var structureAliases = Utils.ResolveStructures(structures);
            var containersKey = "Containers";
            var interfacesKey = "Interfaces";
            var attributesKey = "Attributes";

            foreach (var structureAlias in structureAliases)
            {
                var structure = C4InterFlow.Utils.GetInstance<Structure>(structureAlias);
                if (structure != null)
                {
                    if (structure is SoftwareSystem)
                    {
                        var softwareSystemDictionary = new Dictionary<string, object>();
                        AddStructureFields(softwareSystemDictionary, structure);

                        var softwareSystem = (SoftwareSystem)structure;
                        softwareSystemDictionary["Boundary"] = softwareSystem.Boundary.ToString();

                        var containerAliases = Utils.ResolveStructures(new[] { $"{structure.Alias}.{containersKey}.*" });

                        if (containerAliases.Any())
                        {
                            softwareSystemDictionary[containersKey] = new Dictionary<string, object>();
                            var containersObject = (Dictionary<string, object>)softwareSystemDictionary[containersKey];

                            foreach (var containerAlias in containerAliases)
                            {
                                AddContainer(containersObject, containerAlias);
                            }
                        }

                        var interfaceAliases = Utils.ResolveStructures(new[] { $"{structure.Alias}.{interfacesKey}.*" });

                        if (interfaceAliases.Any())
                        {
                            softwareSystemDictionary[interfacesKey] = new Dictionary<string, object>();
                            var interfacesObject = (Dictionary<string, object>)softwareSystemDictionary[interfacesKey];

                            foreach (var interfaceAlias in interfaceAliases)
                            {
                                AddInterface(interfacesObject, interfaceAlias);
                            }
                        }

                        var attributeAliases = Utils.ResolveStructures(new[] { $"{structure.Alias}.{attributesKey}.*" });

                        if (attributeAliases.Any())
                        {
                            softwareSystemDictionary[attributesKey] = new Dictionary<string, object>();
                            var attributesObject = (Dictionary<string, object>)softwareSystemDictionary[attributesKey];

                            foreach (var attributeAlias in attributeAliases)
                            {
                                AddAttribute(attributesObject, attributeAlias);
                            }
                        }

                        RenderDocuments(
                            softwareSystemDictionary,
                            Path.Join(templatesDirectory, "Software Systems"),
                            Path.Join(outputDirectory, "Software Systems", softwareSystem.Label),
                            fileExtension);
                    }
                    else if(structure is Container)
                    {
                        var container = (Container)structure;
                        var softwareSystem = C4InterFlow.Utils.GetInstance<SoftwareSystem>(container.SoftwareSystem);
                        var containerDictionary = GetContainer(container.Alias);

                        if(containerDictionary != null && softwareSystem != null)
                        {
                            var interfaceAliases = Utils.ResolveStructures(new[] { $"{structure.Alias}.{interfacesKey}.*" });

                            if (interfaceAliases.Any())
                            {
                                containerDictionary[interfacesKey] = new Dictionary<string, object>();
                                var interfacesObject = (Dictionary<string, object>)containerDictionary[interfacesKey];

                                foreach (var interfaceAlias in interfaceAliases)
                                {
                                    AddInterface(interfacesObject, interfaceAlias);
                                }
                            }

                            var attributeAliases = Utils.ResolveStructures(new[] { $"{structure.Alias}.{attributesKey}.*" });

                            if (attributeAliases.Any())
                            {
                                containerDictionary[attributesKey] = new Dictionary<string, object>();
                                var attributesObject = (Dictionary<string, object>)containerDictionary[attributesKey];

                                foreach (var attributeAlias in attributeAliases)
                                {
                                    AddAttribute(attributesObject, attributeAlias);
                                }
                            }

                            RenderDocuments(
                                containerDictionary,
                                Path.Join(templatesDirectory, @"Software Systems\Containers"),
                                Path.Join(outputDirectory, @$"Software Systems\{softwareSystem.Label}\Containers", container.Label),
                                fileExtension);
                        }
                    }
                    else if (structure is Component)
                    {
                        var component = (Component)structure;
                        var container = C4InterFlow.Utils.GetInstance<Container>(component.Container);

                        if(container != null)
                        {
                            var softwareSystem = C4InterFlow.Utils.GetInstance<SoftwareSystem>(container.SoftwareSystem);

                            if (softwareSystem != null)
                            {
                                var componentDictionary = GetComponent(component.Alias);
                                if (componentDictionary != null)
                                {
                                    var interfaceAliases = Utils.ResolveStructures(new[] { $"{structure.Alias}.{interfacesKey}.*" });

                                    if (interfaceAliases.Any())
                                    {
                                        componentDictionary[interfacesKey] = new Dictionary<string, object>();
                                        var interfacesObject = (Dictionary<string, object>)componentDictionary[interfacesKey];

                                        foreach (var interfaceAlias in interfaceAliases)
                                        {
                                            AddInterface(interfacesObject, interfaceAlias);
                                        }
                                    }

                                    RenderDocuments(
                                        componentDictionary,
                                        Path.Join(templatesDirectory, @"Software Systems\Containers\Components"),
                                        Path.Join(outputDirectory, @$"Software Systems\{softwareSystem.Label}\Containers\{container.Label}\Components", component.Label),
                                        fileExtension);
                                }
                            }
                        }
                        
                    }
                }
            }

            Console.WriteLine($"'{COMMAND_NAME}' command completed.");
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"'{COMMAND_NAME}' command failed with exception(s) '{e.Message}'{(e.InnerException != null ? $", '{e.InnerException}'" : string.Empty)}.");
            return 1;
        }
    }

    private static void RenderDocuments(Dictionary<string, object> data, string templatesDirectory, string outputDirectory, string fileExtension)
    {
        var templateFiles = Directory.GetFiles(templatesDirectory, "*.liquid", SearchOption.TopDirectoryOnly);

        foreach (var templateFile in templateFiles)
        {
            var templateSource = File.ReadAllText(templateFile);
            var parser = new FluidParser();
            if (parser.TryParse(templateSource, out var template, out var error))
            {
                var context = new TemplateContext(data);

                var document = template.Render(context);

                Directory.CreateDirectory(outputDirectory);

                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(templateFile);
                var filePath = Path.Join(outputDirectory, $"{fileNameWithoutExtension}.{fileExtension}");

                File.WriteAllText(filePath, document);
            }
            else
            {
                Console.WriteLine($"Error rendering template: {error}");
            }
        }

    }

    private static Dictionary<string, object>? GetComponent(string alias)
    {
        var component = C4InterFlow.Utils.GetInstance<Component>(alias);
        if (component != null)
        {
            var result = new Dictionary<string, object>();
            AddStructureFields(result, component);
            result["ComponentType"] = component.ComponentType.ToString();

            return result;
        }

        return null;
    }

    private static Dictionary<string, object>? GetContainer(string alias)
    {
        var container = C4InterFlow.Utils.GetInstance<Container>(alias);
        if (container != null)
        {
            var result = new Dictionary<string, object>();
            AddStructureFields(result, container);
            result["ContainerType"] = container.ContainerType.ToString();

            return result;
        }

        return null;
    }
    private static void AddContainer(Dictionary<string, object> rootObject, string alias)
    {
        var container = GetContainer(alias);
        if (container != null)
        {
            var containerName = container["Name"].ToString();

            if(!string.IsNullOrEmpty(containerName))
            {
                rootObject[containerName] = container;
            } 
        }
    }

    private static Dictionary<string, object>? GetInterface(string alias)
    {
        var @interface = C4InterFlow.Utils.GetInstance<Interface>(alias);
        if (@interface != null)
        {
            var result = new Dictionary<string, object>();
            AddStructureFields(result, @interface);

            return result;
        }

        return null;
    }
    private static void AddInterface(Dictionary<string, object> rootObject, string alias)
    {
        var @interface = GetInterface(alias);
        if (@interface != null)
        {
            var interfaceName = @interface["Name"].ToString();

            if (!string.IsNullOrEmpty(interfaceName))
            {
                rootObject[interfaceName] = @interface;
            }
        }
    }

    private static Dictionary<string, object>? GetAttribute(string alias)
    {
        var attribute = C4InterFlow.Utils.GetInstance<StructureAttribute>(alias);
        if (attribute != null)
        {
            var result = new Dictionary<string, object>();
            AddStructureFields(result, attribute);
            result["Value"] = attribute.Value;
            return result;
        }

        return null;
    }
    private static void AddAttribute(Dictionary<string, object> rootObject, string alias)
    {
        var attribute = GetAttribute(alias);
        if (attribute != null)
        {
            var interfaceName = attribute["Name"].ToString();

            if (!string.IsNullOrEmpty(interfaceName))
            {
                rootObject[interfaceName] = attribute;
            }
        }
    }
    private static void AddStructureFields(Dictionary<string, object> rootObject, Structure structure)
    {
        rootObject["Name"] = structure.Name;
        rootObject["Alias"] = structure.Alias;
        rootObject["Label"] = structure.Label;
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
