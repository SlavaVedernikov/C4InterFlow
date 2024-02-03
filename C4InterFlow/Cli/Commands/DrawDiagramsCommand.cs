using System.CommandLine;
using C4InterFlow.Diagrams;
using C4InterFlow.Cli.Commands.Options;
using C4InterFlow.Diagrams.Plantuml;
using C4InterFlow.Elements;
using C4InterFlow.Cli.Commands.Binders;
using System.Text.RegularExpressions;
using C4InterFlow.Automation;

namespace C4InterFlow.Cli.Commands;

public class DrawDiagramsCommand : Command
{
    private const string COMMAND_NAME = "draw-diagrams";

    public DrawDiagramsCommand() : base(COMMAND_NAME,
        "Draws diagrams")
    {
        var diagramScopesOption = DiagramScopesOption.Get();
        var diagramTypesOption = DiagramTypesOption.Get();
        var diagramLevelsOfDetailsOption = DiagramLevelsOfDetailsOption.Get();
        var interfacesOption = InterfacesOption.Get();
        var interfacesInputFileOption = InterfacesInputFileOption.Get();
        var businessProcesesOption = BusinessProcesesOption.Get();
        var diagramFormatsOption = DiagramFormatsOption.Get();
        var showBoundariesOption = ShowBoundariesOption.Get();
        var showInterfaceInputAndOutputOption = ShowInterfaceInputAndOutputOption.Get();
        var outputDirectoryOption = OutputDirectoryOption.Get();
        var outputSubDirectoryOption = OutputSubDirectoryOption.Get();
        var diagramNamePrefixOption = DiagramNamePrefixOption.Get();
        var architectureAsCodeInputPathsOption = ArchitectureAsCodeInputPathsOption.Get();
        var architectureAsCodeReaderStrategyTypeOption = ArchitectureAsCodeReaderStrategyTypeOption.Get();

        AddOption(diagramScopesOption);
        AddOption(diagramTypesOption);
        AddOption(diagramLevelsOfDetailsOption);
        AddOption(interfacesOption);
        AddOption(interfacesInputFileOption);
        AddOption(businessProcesesOption);
        AddOption(diagramFormatsOption);
        AddOption(showBoundariesOption);
        AddOption(showInterfaceInputAndOutputOption);
        AddOption(outputDirectoryOption);
        AddOption(outputSubDirectoryOption);
        AddOption(diagramNamePrefixOption);
        AddOption(architectureAsCodeInputPathsOption);
        AddOption(architectureAsCodeReaderStrategyTypeOption);

        this.SetHandler(async (diagramOptions, interfaces, interfacesInputFile, businessProcesses, displayOptions, outputOptions, architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType) =>
            {
                await Execute(diagramOptions, interfaces, interfacesInputFile, businessProcesses, displayOptions, outputOptions, architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType);
            },
            new DiagramOptionsBinder(diagramScopesOption, diagramTypesOption, diagramLevelsOfDetailsOption), 
            interfacesOption,
            interfacesInputFileOption,
            businessProcesesOption,
            new DisplayOptionsBinder(showBoundariesOption, showInterfaceInputAndOutputOption), 
            new OutputOptionsBinder(outputDirectoryOption, outputSubDirectoryOption, diagramNamePrefixOption, diagramFormatsOption),
            architectureAsCodeInputPathsOption,
            architectureAsCodeReaderStrategyTypeOption);
    }

    public static async Task<int> Execute(DiagramOptions diagramOptions, string[]? interfaceAliases, string? interfacesInputFile, string[]? businessProcessTypeNames, DisplayOptions displayOptions, OutputOptions outputOptions, string[] architectureAsCodeInputPaths, string architectureAsCodeReaderStrategyType)
    {
        try
        {
            Console.WriteLine($"{COMMAND_NAME} command is executing...");

            SetArchitectureAsCodeReaderContext(architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType);

            var resolvedInterfaceAliases = new List<string>();
            resolvedInterfaceAliases.AddRange(Utils.ResolveWildcardStructures(interfaceAliases));

            if(!string.IsNullOrEmpty(interfacesInputFile))
            {
                Regex interfaceAliasRegex = new Regex(@"^[^\s]*\.Interfaces\.[^\s]*$");
                var fileInputInterfaceAliases = Utils.ReadLines(interfacesInputFile).Where(x => interfaceAliasRegex.IsMatch(x));
                resolvedInterfaceAliases.AddRange(Utils.ResolveWildcardStructures(fileInputInterfaceAliases));
            }

            var resolvedBusinessProcessTypeNames = Utils.ResolveWildcardStructures(businessProcessTypeNames);

            foreach (var diagramScope in diagramOptions.Scopes)
            {
                var interfaces = GetInterfaces(resolvedInterfaceAliases, diagramScope).ToArray();
                var businessProcesses = GetBusinessProcesses(resolvedBusinessProcessTypeNames, diagramScope).ToArray();

                Console.WriteLine($"Found {interfaces.Count()} interface(s) and {businessProcesses.Count()} business processe(s) for '{diagramScope}' scope.");
                
                if (!interfaces.Any() && !businessProcesses.Any()) continue;

                foreach (var diagramType in diagramOptions.Types)
                {
                    foreach (var levelOfDetails in diagramOptions.LevelsOfDetails)
                    {
                        if (!DiagramOptions.IsSupported(diagramScope, diagramType, levelOfDetails))
                            continue;

                        Console.WriteLine($"Drawing '{diagramType}' diagrams of '{levelOfDetails}' level of details for '{diagramScope}' scope.");
                        switch (diagramType)
                        {
                            case DiagramTypesOption.SEQUENCE:
                                {
                                    DrawSequenceDiagrams(
                                        diagramScope,
                                        levelOfDetails,
                                        interfaces,
                                        outputOptions.Formats,
                                        displayOptions.ShowBoundaries,
                                        displayOptions.ShowInterfaceInputAndOutput,
                                        outputOptions.OutputDirectory,
                                        outputOptions.OutputSubDirectory,
                                        outputOptions.DiagramNamePrefix);

                                    DrawSequenceDiagrams(
                                        diagramScope,
                                        levelOfDetails,
                                        businessProcesses,
                                        outputOptions.Formats,
                                        displayOptions.ShowBoundaries,
                                        displayOptions.ShowInterfaceInputAndOutput,
                                        outputOptions.OutputDirectory,
                                        outputOptions.OutputSubDirectory,
                                        outputOptions.DiagramNamePrefix);
                                    break;
                                }
                            case string c4type
                            when (c4type.Equals(DiagramTypesOption.C4) || c4type.Equals(DiagramTypesOption.C4_STATIC)):
                                {
                                    var isStatic = c4type.Equals(DiagramTypesOption.C4_STATIC);

                                    DrawC4Diagrams(
                                        diagramScope,
                                        levelOfDetails,
                                        interfaces,
                                        outputOptions.Formats,
                                        displayOptions.ShowBoundaries,
                                        displayOptions.ShowInterfaceInputAndOutput,
                                        outputOptions.OutputDirectory,
                                        outputOptions.OutputSubDirectory,
                                        isStatic,
                                        outputOptions.DiagramNamePrefix);

                                    DrawC4Diagrams(
                                        diagramScope,
                                        levelOfDetails,
                                        businessProcesses,
                                        outputOptions.Formats,
                                        displayOptions.ShowBoundaries,
                                        displayOptions.ShowInterfaceInputAndOutput,
                                        outputOptions.OutputDirectory,
                                        outputOptions.OutputSubDirectory,
                                        isStatic);
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
            }
            
            Console.WriteLine($"{COMMAND_NAME} command completed.");
            Console.WriteLine($"See diagram(s) in '{Path.GetFullPath(outputOptions.OutputDirectory)}'");
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Diagram(s) generation failed with exception(s) '{e.Message}'{(e.InnerException !=null ? $", '{e.InnerException}'" : string.Empty)}.");
            return 1;
        }
    }

    private static void SetArchitectureAsCodeReaderContext(string[] architectureAsCodeInputPaths, string architectureAsCodeReaderStrategyType)
    {
        Type strategyType = Type.GetType(architectureAsCodeReaderStrategyType);
        object strategyTypeInstance = Activator.CreateInstance(strategyType);
        var strategyInstance = strategyTypeInstance as IArchitectureAsCodeReaderStrategy;

        if (strategyInstance == null)
        {
            throw new ArgumentException($"'{architectureAsCodeReaderStrategyType}' is not a valid Architecture As Code Reader Strategy type.");
        }

        ArchitectureAsCodeReaderContext.SetCurrentStrategy(strategyInstance, architectureAsCodeInputPaths, new Dictionary<string, string>());
    }

    private static IEnumerable<BusinessProcess> GetBusinessProcesses(IEnumerable<string> businessProcessTypeNames, string scope)
    {
        var result = new List<BusinessProcess>();

        foreach (string businessProcessTypeName in businessProcessTypeNames)
        {
            var businessProcessInstance = C4InterFlow.Utils.GetInstance<BusinessProcess>(businessProcessTypeName);
            if (businessProcessInstance != null)
            {
                result.Add(businessProcessInstance);
            }
            else
            {
                Console.WriteLine($"Could not load Business Process instance for type with name '{businessProcessTypeName}'.");
            }
        }

        return result;
    }

    private static IEnumerable<Interface> GetInterfaces(IEnumerable<string> interfaceAliases, string scope)
    {
        var result = new List<Interface>();
        string pattern = string.Empty;

        switch (scope)
        {
            case DiagramScopesOption.SOFTWARE_SYSTEMS:
            case DiagramScopesOption.SOFTWARE_SYSTEM:
                {
                    // Matches any string that ends with ".Interfaces.<word>"
                    pattern = @"^.*\.Interfaces\.\w+$";
                    break;
                }
            case DiagramScopesOption.SOFTWARE_SYSTEM_INTERFACE:
                {
                    // Matches any string that ends with ".Interfaces.<word>" and does not contain ".Containers." or ".Components."
                    pattern = @"^(?!.*\.(Containers|Components)\.).*\.Interfaces\.\w+$";
                    break;
                }
            case DiagramScopesOption.CONTAINER:
                {
                    // Matches any string that contains ".Containers.<word>" or ".Components.<word>", and ends with ".Interfaces.<word>"
                    pattern = @"^.*\.(Containers|Components)\.\w+.*\.Interfaces\.\w+$";
                    break;
                }
            case DiagramScopesOption.CONTAINER_INTERFACE:
                {
                    // Matches any string that ends with ".Containers.<word>.Interfaces.<word>"
                    pattern = @"^.*\.Containers\.\w+\.Interfaces\.\w+$";
                    break;
                }
            case DiagramScopesOption.COMPONENT:
            case DiagramScopesOption.COMPONENT_INTERFACE:
                {
                    // Matches any string that ends with ".Components.<word>.Interfaces.<word>"
                    pattern = @"^.*\.Components\.\w+\.Interfaces\.\w+$";
                    break;
                }
            default:
                break;
        }


        if (!string.IsNullOrEmpty(pattern))
        {
            foreach (string interfaceAlias in interfaceAliases
                .Where(x => Regex.IsMatch(x, pattern)))
            {
                var interfaceInstance = C4InterFlow.Utils.GetInstance<Interface>(interfaceAlias);
                if (interfaceInstance != null)
                {
                    result.Add(interfaceInstance);
                }
                else
                {
                    Console.WriteLine($"Could not load Interface instance for structure with alias '{interfaceAlias}'.");
                }
            }
        }
        

        return result;
    }

    private static Diagram GetDiagram(string levelOfDetails, BusinessProcess businessProcess, bool showBoundaries, bool showInterfaceInputAndOutput, bool isStatic = false)
    {
        var result = default(Diagram);

        var diagramTitle = GetDiagramTitle(businessProcess, levelOfDetails);
        if (diagramTitle != null)
        {
            switch (levelOfDetails)
            {
                case DiagramLevelsOfDetailsOption.COMPONENT:
                    {
                        result = new ComponentDiagram(
                            diagramTitle,
                            process: businessProcess,
                            showBoundaries: showBoundaries,
                            showInterfaceInputAndOutput: showInterfaceInputAndOutput,
                            isStatic : isStatic).Build();
                        break;
                    }
                case DiagramLevelsOfDetailsOption.CONTAINER:
                    {
                        result = new ContainerDiagram(
                            diagramTitle,
                            process: businessProcess,
                            showBoundaries: showBoundaries,
                            isStatic: isStatic).Build();
                        break;
                    }
                case DiagramLevelsOfDetailsOption.CONTEXT:
                    {
                        result = new ContextDiagram(
                            diagramTitle,
                            process: businessProcess,
                            isStatic: isStatic).Build();
                        break;
                    }
                default:
                    break;
            }
        }
        else
        {
            Console.WriteLine($"Could not generate diagram title for business process '{businessProcess?.Label}'.");
        }

        return result;
    }

    private static Diagram GetDiagram(string levelOfDetails, Interface[] interfaces, bool showBoundaries, bool showInterfaceInputAndOutput, bool isStatic = false)
    {
        var result = default(Diagram);

        var diagramTitle = $"C4{(isStatic ? " Static" : string.Empty)} - {levelOfDetails.ToUpper()} level";

        var flow = new Flow();

        foreach (var @interface in interfaces)
        {
            flow.Use(@interface.Alias);
        }

        var process = new BusinessProcess(
                        new BusinessActivity[] {
                            new BusinessActivity(flow)
                        });

        switch (levelOfDetails)
        {
            case DiagramLevelsOfDetailsOption.COMPONENT:
                {
                    result = new ComponentDiagram(
                        diagramTitle,
                        process: process,
                        showBoundaries: showBoundaries,
                        showInterfaceInputAndOutput: showInterfaceInputAndOutput,
                        isStatic: isStatic).Build();
                    break;
                }
            case DiagramLevelsOfDetailsOption.CONTAINER:
                {
                    result = new ContainerDiagram(
                        diagramTitle,
                        process: process,
                        showBoundaries: showBoundaries,
                        isStatic: isStatic).Build();
                    break;
                }
            case DiagramLevelsOfDetailsOption.CONTEXT:
                {
                    result = new ContextDiagram(
                        diagramTitle,
                        process: process,
                        isStatic: isStatic).Build();
                    break;
                }
            default:
                break;
        }

        return result;
    }

    private static Diagram GetDiagram(string levelOfDetails, Interface @interface, bool showBoundaries, bool showInterfaceInputAndOutput, bool isStatic = false)
    {
        var result = default(Diagram);

        var diagramTitle = GetDiagramTitle(@interface, levelOfDetails);
        if (diagramTitle != null)
        {
            var process = new BusinessProcess(
                            new BusinessActivity[] {
                            new BusinessActivity(
                                new Flow(C4InterFlow.Utils.ExternalSystem.ALIAS).Use(@interface.Alias))
                            });
            switch (levelOfDetails)
            {
                case DiagramLevelsOfDetailsOption.COMPONENT:
                    {
                        result = new ComponentDiagram(
                            diagramTitle,
                            process: process,
                            showBoundaries: showBoundaries,
                            showInterfaceInputAndOutput: showInterfaceInputAndOutput,
                            isStatic: isStatic).Build();
                        break;
                    }
                case DiagramLevelsOfDetailsOption.CONTAINER:
                    {
                        result = new ContainerDiagram(
                            diagramTitle,
                            process: process,
                            showBoundaries: showBoundaries,
                            isStatic: isStatic).Build();
                        break;
                    }
                case DiagramLevelsOfDetailsOption.CONTEXT:
                    {
                        result = new ContextDiagram(
                            diagramTitle,
                            process: process).Build();
                        break;
                    }
                default:
                    break;
            }
        }
        else
        {
            Console.WriteLine($"Could not generate diagram title for interface with alias '{@interface.Alias}'.");
        }


        return result;
    }

    private static void ClearDirectory(string path)
    {
        if(Directory.Exists(path))
        {
            Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
    }

    private static void DrawSequenceDiagrams(string scope, string levelOfDetails, BusinessProcess[] businessProcesses, string[] formats, bool showBoundaries, bool showInterfaceInputAndOutput, string outputDirectory, string? outputSubDirectory = null, string? diagramNamePrefix = null)
    {
        if (!businessProcesses.Any()) return;

        var context = new PlantumlSequenceContext();
        if (formats.Contains(DiagramFormatsOption.PNG))
        {
            context.UseDiagramImageBuilder();
        }
        if (formats.Contains(DiagramFormatsOption.SVG))
        {
            context.UseDiagramSvgImageBuilder();
        }
        if (formats.Contains(DiagramFormatsOption.MD))
        {
            context.UseDiagramMdDocumentBuilder();
        }

        Parallel.ForEach(businessProcesses, businessProcess =>
        {
            var diagram = GetDiagram(levelOfDetails, businessProcess, showBoundaries, showInterfaceInputAndOutput);

            if (TryGetDiagramPath(
                    scope,
                    levelOfDetails,
                    DiagramTypesOption.SEQUENCE,
                    businessProcess,
                    out var path,
                    out var fileName,
                    outputSubDirectory,
                    diagramNamePrefix))
            {
                context.Export(outputDirectory, diagram, path, fileName);
            }
        });
        
    }

    private static void DrawC4Diagrams(string scope, string levelOfDetails, BusinessProcess[] businessProcesses, string[] formats, bool showBoundaries, bool showInterfaceInputAndOutput, string outputDirectory, string? outputSubDirectory = null, bool isStatic = false, string? diagramNamePrefix = null)
    {
        if (!businessProcesses.Any()) return;

        var context = new PlantumlContext();
        if (formats.Contains(DiagramFormatsOption.PNG))
        {
            context.UseDiagramImageBuilder();
        }
        if (formats.Contains(DiagramFormatsOption.SVG))
        {
            context.UseDiagramSvgImageBuilder();
        }
        if (formats.Contains(DiagramFormatsOption.MD))
        {
            context.UseDiagramMdDocumentBuilder();
        }

        Parallel.ForEach(businessProcesses, businessProcess =>
        {
            var diagram = GetDiagram(levelOfDetails, businessProcess, showBoundaries, showInterfaceInputAndOutput, isStatic);

            if (TryGetDiagramPath(
                    scope,
                    levelOfDetails,
                    isStatic ? DiagramTypesOption.C4_STATIC : DiagramTypesOption.C4,
                    businessProcess,
                    out var path,
                    out var fileName,
                    outputSubDirectory,
                    diagramNamePrefix))
            {
                context.Export(outputDirectory, diagram, path, fileName);
            }
        });
    }

    private static void DrawSequenceDiagrams(string scope, string levelOfDetails, Interface[] interfaces, string[] formats, bool showBoundaries, bool showInterfaceInputAndOutput, string outputDirectory, string? outputSubDirectory = null, string? diagramNamePrefix = null)
    {
        if (!interfaces.Any()) return;

        var context = new PlantumlSequenceContext();
        if (formats.Contains(DiagramFormatsOption.PNG))
        {
            context.UseDiagramImageBuilder();
        }
        if (formats.Contains(DiagramFormatsOption.SVG))
        {
            context.UseDiagramSvgImageBuilder();
        }
        if (formats.Contains(DiagramFormatsOption.MD))
        {
            context.UseDiagramMdDocumentBuilder();
        }

        Parallel.ForEach(interfaces, @interface =>
        {
            var diagram = GetDiagram(levelOfDetails, @interface, showBoundaries, showInterfaceInputAndOutput);

            if (TryGetDiagramPath(
                    scope,
                    levelOfDetails,
                    DiagramTypesOption.SEQUENCE,
                    @interface,
                    out var path,
                    out var fileName,
                    outputSubDirectory,
                    diagramNamePrefix))
            {
                context.Export(outputDirectory, diagram, path, fileName);
            }
        });
    }

    private static void DrawC4Diagrams(string scope, string levelOfDetails, Interface[] interfaces, string[] formats, bool showBoundaries, bool showInterfaceInputAndOutput, string outputDirectory, string? outputSubDirectory = null, bool isStatic = false, string? diagramNamePrefix = null)
    {
        if (!interfaces.Any()) return;

        var context = new PlantumlContext();
        if (formats.Contains(DiagramFormatsOption.PNG))
        {
            context.UseDiagramImageBuilder();
        }
        if (formats.Contains(DiagramFormatsOption.SVG))
        {
            context.UseDiagramSvgImageBuilder();
        }
        if (formats.Contains(DiagramFormatsOption.MD))
        {
            context.UseDiagramMdDocumentBuilder();
        }

        if (scope == DiagramScopesOption.SOFTWARE_SYSTEMS)
        {
            var diagram = GetDiagram(levelOfDetails, interfaces, showBoundaries, showInterfaceInputAndOutput, isStatic);
            if (TryGetDiagramPath(
                    scope,
                    levelOfDetails,
                    isStatic ? DiagramTypesOption.C4_STATIC : DiagramTypesOption.C4,
                    out var path,
                    out var fileName,
                    outputSubDirectory,
                    diagramNamePrefix))
            {
                context.Export(outputDirectory, diagram, path, fileName);
            }
        }
        else if (scope == DiagramScopesOption.SOFTWARE_SYSTEM)
        {
            string pattern = @"^(.*?)(?:\.Interfaces|\.Containers)";
            var softwareSystemAliases = interfaces.Select(x => Regex.Match(x.Alias, pattern))
                  .Where(m => m.Success)
                  .Select(m => m.Groups[1].Value)
                  .Distinct();

            Parallel.ForEach(softwareSystemAliases, softwareSystemAlias =>
            {
                var systemInterfaces = interfaces.Where(x => x.Alias.StartsWith(softwareSystemAlias)).ToArray();
                var diagram = GetDiagram(levelOfDetails, systemInterfaces, showBoundaries, showInterfaceInputAndOutput, isStatic);
                if (TryGetDiagramPath(
                        scope,
                        levelOfDetails,
                        isStatic ? DiagramTypesOption.C4_STATIC : DiagramTypesOption.C4,
                        systemInterfaces.First(),
                        out var path,
                        out var fileName,
                        outputSubDirectory,
                        diagramNamePrefix))
                {
                    context.Export(outputDirectory, diagram, path, fileName);
                }

            });
        }
        else if (scope == DiagramScopesOption.CONTAINER)
        {
            string pattern = @"(.*?\.Containers\.[^.]+)";
            var containerAliases = interfaces.Select(x => Regex.Match(x.Alias, pattern))
                  .Where(m => m.Success)
                  .Select(m => m.Groups[1].Value)
                  .Distinct();

            Parallel.ForEach(containerAliases, containerAlias =>
            {
                var containerInterfaces = interfaces.Where(x => x.Alias.StartsWith(containerAlias)).ToArray();
                var diagram = GetDiagram(levelOfDetails, containerInterfaces, showBoundaries, showInterfaceInputAndOutput, isStatic);
                if (TryGetDiagramPath(
                        scope,
                        levelOfDetails,
                        isStatic ? DiagramTypesOption.C4_STATIC : DiagramTypesOption.C4,
                        containerInterfaces.First(),
                        out var path,
                        out var fileName,
                        outputSubDirectory,
                        diagramNamePrefix))
                {
                    context.Export(outputDirectory, diagram, path, fileName);
                }

            });
        }
        else if (scope == DiagramScopesOption.COMPONENT)
        {
            string pattern = @"(.*?\.Components\.[^.]+)";
            var componentAliases = interfaces.Select(x => Regex.Match(x.Alias, pattern))
                  .Where(m => m.Success)
                  .Select(m => m.Groups[1].Value)
                  .Distinct();

            Parallel.ForEach(componentAliases, componentAlias =>
            {
                var componentInterfaces = interfaces.Where(x => x.Alias.StartsWith(componentAlias)).ToArray();
                var diagram = GetDiagram(levelOfDetails, componentInterfaces, showBoundaries, showInterfaceInputAndOutput, isStatic);
                if (TryGetDiagramPath(
                        scope,
                        levelOfDetails,
                        isStatic ? DiagramTypesOption.C4_STATIC : DiagramTypesOption.C4,
                        componentInterfaces.First(),
                        out var path,
                        out var fileName,
                        outputSubDirectory,
                        diagramNamePrefix))
                {
                    context.Export(outputDirectory, diagram, path, fileName);
                }

            });
        }
        else
        {
            Parallel.ForEach(interfaces, @interface =>
            {
                var diagram = GetDiagram(levelOfDetails, @interface, showBoundaries, showInterfaceInputAndOutput, isStatic);

                if (TryGetDiagramPath(
                    scope,
                    levelOfDetails,
                    isStatic ? DiagramTypesOption.C4_STATIC : DiagramTypesOption.C4,
                    @interface,
                    out var path,
                    out var fileName,
                    outputSubDirectory,
                    diagramNamePrefix))
                {
                    context.Export(outputDirectory, diagram, path, fileName);
                }
            });
        }
    }

    private static string? GetDiagramTitle(BusinessProcess? businessProcess, string levelOfDetails)
    {
        if (string.IsNullOrEmpty(businessProcess?.Label))
            return null;

        else  
            return $"{businessProcess.Label} - {levelOfDetails.ToUpper()} level)";
    }

    private static bool TryGetDiagramPath(string scope, string levelOfDetails, string diagramType, out string path, out string fileName, string? outputSubDirectory = null, string? diagramNamePrefix = null)
    {
        switch (scope)
        {
            case DiagramScopesOption.SOFTWARE_SYSTEMS:
            {
                if (!string.IsNullOrEmpty(outputSubDirectory))
                {
                    path = outputSubDirectory;
                }
                else
                {
                    path = "Software Systems";
                }

                fileName = $"{(!string.IsNullOrEmpty(diagramNamePrefix) ? $"{diagramNamePrefix} - " : string.Empty)}{ToPrettyName(levelOfDetails)} {ToPrettyName(diagramType)}.puml";

                break;
            }
            
            default:
                {
                    path = fileName = null;
                    break;
                }
                
        }

        return !string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(fileName);
    }
    private static bool TryGetDiagramPath(string scope, string levelOfDetails, string diagramType, Interface @interface, out string path, out string fileName, string? outputSubDirectory = null, string? diagramNamePrefix = null)
    {
        if (!string.IsNullOrEmpty(outputSubDirectory))
        {
            path = outputSubDirectory;
        }
        else
        {
            path = "Software Systems";
        }

        fileName = $"{(!string.IsNullOrEmpty(diagramNamePrefix) ? $"{diagramNamePrefix} - " : string.Empty)}{ToPrettyName(levelOfDetails)} {ToPrettyName(diagramType)}.puml";
        
        switch (scope)
        {
            case DiagramScopesOption.SOFTWARE_SYSTEM:
                {
                    if (!TryParseInterface(@interface, out var softwareSystem))
                    {
                        Console.WriteLine($"Could not get Software System for interface with alias '{@interface.Alias}'.");
                        path = fileName = null;
                    }
                    else
                    {
                        path = Path.Join(path, softwareSystem.Label);
                    }
                    break;
                }
            case DiagramScopesOption.SOFTWARE_SYSTEM_INTERFACE:
                {
                    if (!TryParseInterface(@interface, out var softwareSystem))
                    {
                        Console.WriteLine($"Could not get Software System for interface with alias '{@interface.Alias}'.");
                        path = fileName = null;
                    }
                    else
                    {
                        path = Path.Join(path, softwareSystem.Label, "Interfaces", @interface.Label);
                    }

                    break;
                }
            case DiagramScopesOption.CONTAINER:
                {
                    if (!TryParseInterface(@interface, out var softwareSystem, out var container))
                    {
                        Console.WriteLine($"Could not get {(softwareSystem == null ? "Software System" : "Container")} for interface with alias '{@interface.Alias}'.");
                        path = fileName = null;
                    }
                    else
                    {
                        path = Path.Join(path, softwareSystem.Label, "Containers", container.Label);
                    }

                    break;
                }
            case DiagramScopesOption.CONTAINER_INTERFACE:
                {
                    if (!TryParseInterface(@interface, out var softwareSystem, out var container))
                    {
                        Console.WriteLine($"Could not get {(softwareSystem == null ? "Software System" : "Container")} for interface with alias '{@interface.Alias}'.");
                        path = fileName = null;
                    }
                    else
                    {
                        path = Path.Join(path, softwareSystem.Label, "Containers", container.Label, "Interfaces", @interface.Label);
                    }
                    
                    break;
                }
            case DiagramScopesOption.COMPONENT:
                {
                    if (!TryParseInterface(@interface, out var softwareSystem, out var container, out var component))
                    {
                        Console.WriteLine($"Could not get {(softwareSystem == null ? "Software System" : (container == null ? "Container" : "Component"))} for interface with alias '{@interface.Alias}'.");
                        path = fileName = null;
                    }
                    else
                    {
                        path = Path.Join(path, softwareSystem.Label, "Containers", container.Label, "Components", component.Label);
                    }
                    break;
                }
            case DiagramScopesOption.COMPONENT_INTERFACE:
                {
                    if (!TryParseInterface(@interface, out var softwareSystem, out var container, out var component))
                    {
                        Console.WriteLine($"Could not get {(softwareSystem == null ? "Software System" : (container == null ? "Container" : "Component"))} for interface with alias '{@interface.Alias}'.");
                        path = fileName = null;
                    }
                    else
                    {
                        path = Path.Join(path, softwareSystem.Label, "Containers", container.Label, "Components", component.Label, "Interfaces", @interface.Label);
                    }
                    break;
                }
            default:
                {
                    path = fileName = null;
                    break;
                }
        }

        return !string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(fileName);
    }

    private static string ToPrettyName(string str)
    {
        string result = Regex.Replace(str, @"\b[a-z]", m => m.Value.ToUpper());

        result = Regex.Replace(result, @"-", " ");

        return result;
    }

    private static bool TryGetDiagramPath(string scope, string levelOfDetails, string diagramType, BusinessProcess businessProcess, out string path, out string fileName, string? outputSubDirectory = null, string? diagramNamePrefix = null)
    {
        
        if(!string.IsNullOrEmpty(outputSubDirectory))
        {
            path = outputSubDirectory;
        }
        else
        {
            path = $"Business Processes";
        }

        fileName = $"{(!string.IsNullOrEmpty(diagramNamePrefix) ? $"{diagramNamePrefix} - " : string.Empty)}{ToPrettyName(levelOfDetails)} {ToPrettyName(diagramType)}.puml";

        switch (scope)
        {
            case DiagramScopesOption.BUSINESS_PROCESS:
                {
                    path = Path.Join(path, businessProcess.Label);
                    break;
                }
            default:
                {
                    path = fileName = null;
                    break;
                }
        }

        return !string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(fileName);
    }
    private static string? GetDiagramTitle(Interface? @interface, string levelOfDetails)
    {
        if (string.IsNullOrEmpty(@interface?.Alias))
            return null;

        if (TryParseInterface(@interface, out var system, out var container, out var component))
        {
            return $"{system.Label}{(container != null ? $" - {container.Name}" : string.Empty)}{(component != null ? $" - {component.Name}" : string.Empty)} - {@interface.Name} - {levelOfDetails.ToUpper()} level";
        }

        return null;
    }

    private static bool TryParseInterface(Interface @interface, out SoftwareSystem? system, out Container? container, out Component? component)
    {
        TryParseAlias(@interface.Alias, out system, out container, out component);

        return (system as Structure ?? container as Structure ?? component as Structure) != null;
    }

    private static bool TryParseInterface(Interface @interface, out SoftwareSystem? system, out Container? container)
    {
        TryParseAlias(@interface.Alias, out system, out container, out var tempComponent);

        return (system as Structure ?? container as Structure) != null;
    }

    private static bool TryParseInterface(Interface @interface, out SoftwareSystem? system)
    {
        TryParseAlias(@interface.Alias, out system, out var tempContainer, out var tempComponent);

        return (system as Structure) != null;
    }

    private static bool TryParseAlias(string interfaceAlias, out SoftwareSystem? system, out Container? container, out Component? component)
    {
        container = default(Container);
        system = default(SoftwareSystem);
        component = default(Component);

        var interfacesSegment = ".Interfaces.";
        if (interfaceAlias == null || !interfaceAlias.Contains(interfacesSegment)) return false;

        try
        {
            if(interfaceAlias.Contains("Components"))
            {
                var componentAlias = interfaceAlias.Substring(0, interfaceAlias.IndexOf(interfacesSegment));
                component = C4InterFlow.Utils.GetInstance<Component>(componentAlias);
                container = C4InterFlow.Utils.GetInstance<Container>(component?.Container);
                system = C4InterFlow.Utils.GetInstance<SoftwareSystem>(container?.SoftwareSystem);
            }
            else if(interfaceAlias.Contains("Containers"))
            {
                var containerAlias = interfaceAlias.Substring(0, interfaceAlias.IndexOf(interfacesSegment));
                container = C4InterFlow.Utils.GetInstance<Container>(containerAlias);
                system = C4InterFlow.Utils.GetInstance<SoftwareSystem>(container?.SoftwareSystem);
            }
            else
            {
                var softwareSystemAlias = interfaceAlias.Substring(0, interfaceAlias.IndexOf(interfacesSegment));
                system = C4InterFlow.Utils.GetInstance<SoftwareSystem>(softwareSystemAlias);
            }

            return (system as Structure ?? container as Structure ?? component as Structure) != null;
        }
        catch
        {
            return false;
        }

    }
}
