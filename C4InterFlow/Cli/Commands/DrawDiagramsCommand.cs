using System.CommandLine;
using C4InterFlow.Visualisation;
using C4InterFlow.Cli.Commands.Options;
using C4InterFlow.Visualisation.Plantuml;
using C4InterFlow.Structures;
using C4InterFlow.Cli.Commands.Binders;
using System.Text.RegularExpressions;
using C4InterFlow.Automation;
using C4InterFlow.Commons.Extensions;
using C4InterFlow.Visualisation.Plantuml.Enums;
using Parlot.Fluent;
using Fluid.Ast;
using Serilog;
using static C4InterFlow.SoftwareSystems.ExternalSystem;
using C4InterFlow.Structures.Views;

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
        var namespacesOption = NamespacesOption.Get();
        var activitiesOption = ActivitiesOption.Get();
        var diagramFormatsOption = DiagramFormatsOption.Get();
        var showBoundariesOption = ShowBoundariesOption.Get();
        var showInterfaceInputAndOutputOption = ShowInterfaceInputAndOutputOption.Get();
        var outputDirectoryOption = OutputDirectoryOption.Get();
        var diagramNamePrefixOption = DiagramNamePrefixOption.Get();
        var architectureAsCodeInputPathsOption = AaCInputPathsOption.Get();
        var architectureAsCodeReaderStrategyTypeOption = AaCReaderStrategyTypeOption.Get();
        var diagramMaxLineLabelsOption = DiagramMaxLineLabelsOption.Get();
        var expandUpstreamOption = ExpandUpstreamOption.Get();

        AddOption(diagramScopesOption);
        AddOption(diagramTypesOption);
        AddOption(diagramLevelsOfDetailsOption);
        AddOption(interfacesOption);
        AddOption(interfacesInputFileOption);
        AddOption(businessProcesesOption);
        AddOption(namespacesOption);
        AddOption(activitiesOption);
        AddOption(diagramFormatsOption);
        AddOption(showInterfaceInputAndOutputOption);
        AddOption(outputDirectoryOption);
        AddOption(diagramNamePrefixOption);
        AddOption(architectureAsCodeInputPathsOption);
        AddOption(architectureAsCodeReaderStrategyTypeOption);
        AddOption(diagramMaxLineLabelsOption);
        AddOption(expandUpstreamOption);


        this.SetHandler(async (diagramOptions, inputOptions, displayOptions, outputOptions, architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType ) =>
            {
                if (!AaCReaderContext.HasStrategy)
                {
                    Utils.SetArchitectureAsCodeReaderContext(architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType);
                }

                await Execute(diagramOptions, inputOptions, displayOptions, outputOptions);
            },
            new DiagramOptionsBinder(
                diagramScopesOption, 
                diagramTypesOption, 
                diagramLevelsOfDetailsOption), 
            new InputOptionsBinder(
                interfacesOption,
                interfacesInputFileOption,
                businessProcesesOption,
                namespacesOption,
                activitiesOption),
            new DisplayOptionsBinder(
                showInterfaceInputAndOutputOption,
                diagramMaxLineLabelsOption,
                expandUpstreamOption), 
            new OutputOptionsBinder(
                outputDirectoryOption,
                diagramNamePrefixOption,
                diagramFormatsOption),
            architectureAsCodeInputPathsOption,
            architectureAsCodeReaderStrategyTypeOption);
    }

    public static async Task<int> Execute(DiagramOptions diagramOptions, InputOptions inputOptions, DisplayOptions displayOptions, OutputOptions outputOptions)
    {
        try
        {
            Log.Information("{Name} command is executing", COMMAND_NAME);

            
            AaCReaderContext.Strategy.Validate(out var errors);

            if(errors.Any())
            {
                foreach (var validationError in errors)
                {
                    Log.Error(validationError.Template, validationError.Args);
                }
                
                throw new InvalidDataException("AaC has errors. Please resolve and retry.");
            }
            var resolvedInterfaceAliases = new List<string>();
            resolvedInterfaceAliases.AddRange(Utils.ResolveStructures(inputOptions.Interfaces));

            if(!string.IsNullOrEmpty(inputOptions.InterfacesInputFile))
            {
                Regex interfaceAliasRegex = new Regex(@"^[^\s]*\.Interfaces\.[^\s]*$");
                var fileInputInterfaceAliases = Utils.ReadLines(inputOptions.InterfacesInputFile).Where(x => interfaceAliasRegex.IsMatch(x));
                resolvedInterfaceAliases.AddRange(Utils.ResolveStructures(fileInputInterfaceAliases));
            }

            resolvedInterfaceAliases = resolvedInterfaceAliases.Distinct().ToList();

            var resolvedBusinessProcessAliases = Utils.ResolveStructures(inputOptions.BusinessProcesses).Distinct();

            var resolvedActivityAliases = Utils.ResolveStructures(inputOptions.Activities).Distinct();

            Log.Information("Discovering Business Processes");
            var businessProcesses = GetBusinessProcesses(resolvedBusinessProcessAliases).ToArray();
            Log.Information("Found {ProcessesCount} business process(es)", businessProcesses.Length);

            Log.Information("Discovering Activities");
            var activities = GetActivities(resolvedActivityAliases).ToArray();
            Log.Information("Found {ActivityCount} activities", activities.Length);

            foreach (var diagramScope in diagramOptions.Scopes)
            {
                Log.Information("Discovering Interfaces for {Scope} diagram scope", diagramScope);
                var interfaces = await GetInterfaces(resolvedInterfaceAliases, diagramScope);

                Log.Information("Found {InterfacesCount} interface(s) for {Scope}", interfaces.Count(), diagramScope);

                if (!interfaces.Any() && !businessProcesses.Any() && !activities.Any()) continue;

                foreach (var diagramType in diagramOptions.Types)
                {
                    foreach (var levelOfDetails in diagramOptions.LevelsOfDetails)
                    {
                        if (!DiagramOptions.IsSupported(diagramScope, diagramType, levelOfDetails))
                            continue;

                        Log.Information("Drawing {DiagramType} diagrams of {Level} level of details for {Scope} scope", diagramType, levelOfDetails, diagramScope);

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
                                        outputOptions.SubtractPath,
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
                                        outputOptions.SubtractPath,
                                        outputOptions.DiagramNamePrefix);

                                    DrawSequenceDiagrams(
                                        diagramScope,
                                        levelOfDetails,
                                        activities,
                                        outputOptions.Formats,
                                        displayOptions.ShowBoundaries,
                                        displayOptions.ShowInterfaceInputAndOutput,
                                        outputOptions.OutputDirectory,
                                        outputOptions.OutputSubDirectory,
                                        outputOptions.SubtractPath,
                                        outputOptions.DiagramNamePrefix);
                                    break;
                                }
                            case DiagramTypesOption.C4_SEQUENCE:
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
                                        outputOptions.SubtractPath,
                                        outputOptions.DiagramNamePrefix,
                                        SequenceDiagramStyle.C4);

                                    DrawSequenceDiagrams(
                                        diagramScope,
                                        levelOfDetails,
                                        businessProcesses,
                                        outputOptions.Formats,
                                        displayOptions.ShowBoundaries,
                                        displayOptions.ShowInterfaceInputAndOutput,
                                        outputOptions.OutputDirectory,
                                        outputOptions.OutputSubDirectory,
                                        outputOptions.SubtractPath,
                                        outputOptions.DiagramNamePrefix,
                                        SequenceDiagramStyle.C4);

                                    DrawSequenceDiagrams(
                                        diagramScope,
                                        levelOfDetails,
                                        activities,
                                        outputOptions.Formats,
                                        displayOptions.ShowBoundaries,
                                        displayOptions.ShowInterfaceInputAndOutput,
                                        outputOptions.OutputDirectory,
                                        outputOptions.OutputSubDirectory,
                                        outputOptions.SubtractPath,
                                        outputOptions.DiagramNamePrefix,
                                        SequenceDiagramStyle.C4);
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
                                        inputOptions.Namespaces,
                                        outputOptions.Formats,
                                        displayOptions.ShowBoundaries,
                                        displayOptions.ShowInterfaceInputAndOutput,
                                        displayOptions.MaxLineLabels,
                                        outputOptions.OutputDirectory,
                                        outputOptions.OutputSubDirectory,
                                        outputOptions.SubtractPath,
                                        isStatic,
                                        outputOptions.DiagramNamePrefix,
                                        displayOptions.ExpandUpstream);

                                    DrawC4Diagrams(
                                        diagramScope,
                                        levelOfDetails,
                                        businessProcesses,
                                        outputOptions.Formats,
                                        displayOptions.ShowBoundaries,
                                        displayOptions.ShowInterfaceInputAndOutput,
                                        displayOptions.MaxLineLabels,
                                        outputOptions.OutputDirectory,
                                        outputOptions.OutputSubDirectory,
                                        outputOptions.SubtractPath,
                                        isStatic);

                                    DrawC4Diagrams(
                                        diagramScope,
                                        levelOfDetails,
                                        activities,
                                        outputOptions.Formats,
                                        displayOptions.ShowBoundaries,
                                        displayOptions.ShowInterfaceInputAndOutput,
                                        displayOptions.MaxLineLabels,
                                        outputOptions.OutputDirectory,
                                        outputOptions.OutputSubDirectory,
                                        outputOptions.SubtractPath,
                                        isStatic);
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
            }
            
            Log.Information("{Name} command completed", COMMAND_NAME);
            Log.Information("See diagram(s) in {Path}", Path.GetFullPath(outputOptions.OutputDirectory));
            return 0;
        }
        catch (Exception e)
        {
            Log.Error(e,"{Name} command failed with exception(s) {Error}", COMMAND_NAME, $"{e.Message}{(e.InnerException !=null ? $", {e.InnerException}" : string.Empty)}");
            return 1;
        }
    }

    private static IEnumerable<BusinessProcess> GetBusinessProcesses(IEnumerable<string> businessProcessAliases)
    {
        var result = new List<BusinessProcess>();

        foreach (string businessProcessAlias in businessProcessAliases)
        {
            var businessProcessInstance = C4InterFlow.Utils.GetInstance<BusinessProcess>(businessProcessAlias);
            if (businessProcessInstance != null)
            {
                result.Add(businessProcessInstance);
            }
            else
            {
                Log.Warning("Could not load Business Process instance with alias {ProcessAlias}", businessProcessAlias);
            }
        }

        return result;
    }

    private static IEnumerable<Activity> GetActivities(IEnumerable<string> activityAliases)
    {
        var result = new List<Activity>();

        foreach (string activityAlias in activityAliases)
        {
            var activityInstance = C4InterFlow.Utils.GetInstance<Activity>(activityAlias);
            if (activityInstance != null)
            {
                result.Add(activityInstance);
            }
            else
            {
                Log.Warning("Could not load Activity instance with alias {ActivityAlias}", activityAlias);
            }
        }

        return result;
    }

    private static async Task<IEnumerable<Interface>> GetInterfaces(IEnumerable<string> interfaceAliases, string scope)
    {
        var result = new List<Interface>();
        string pattern = string.Empty;

        switch (scope)
        {
            case DiagramScopesOption.ALL_SOFTWARE_SYSTEMS:
            case DiagramScopesOption.NAMESPACE:
            case DiagramScopesOption.NAMESPACE_SOFTWARE_SYSTEMS:
            case DiagramScopesOption.AUTO:
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
            var scopedInterfaceAliases = interfaceAliases
                .Where(x => Regex.IsMatch(x, pattern));

            foreach (string interfaceAlias in scopedInterfaceAliases)
            {
                var interfaceInstance = C4InterFlow.Utils.GetInstance<Interface>(interfaceAlias);
                if (interfaceInstance != null)
                {
                    result.Add(interfaceInstance);
                }
                else
                {
                    Log.Warning("Could not load Interface instance for structure with alias {Alias}", interfaceAlias);
                }
            }
        }
        

        return result;
    }

    private static Diagram GetDiagram(string diagramType, string levelOfDetails, BusinessProcess businessProcess, bool showBoundaries, bool showInterfaceInputAndOutput, int maxLineLabels = DiagramMaxLineLabelsOption.DefaultValue, bool isStatic = false)
    {
        var result = default(Diagram);

        var diagramTitle = GetDiagramTitle(businessProcess, levelOfDetails, diagramType);
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
                            maxLineLabels: maxLineLabels,
                            isStatic : isStatic).Build();
                        break;
                    }
                case DiagramLevelsOfDetailsOption.CONTAINER:
                    {
                        result = new ContainerDiagram(
                            diagramTitle,
                            process: businessProcess,
                            showBoundaries: showBoundaries,
                            maxLineLabels: maxLineLabels,
                            isStatic: isStatic).Build();
                        break;
                    }
                case DiagramLevelsOfDetailsOption.CONTEXT:
                    {
                        result = new ContextDiagram(
                            diagramTitle,
                            process: businessProcess,
                            maxLineLabels: maxLineLabels,
                            isStatic: isStatic).Build();
                        break;
                    }
                default:
                    break;
            }
        }
        else
        {
            Log.Warning("Could not generate diagram title for business process {Name}", businessProcess.Label);
        }

        return result;
    }

    private static Diagram GetDiagram(string diagramType, string levelOfDetails, Activity activity, bool showBoundaries, bool showInterfaceInputAndOutput, int maxLineLabels = DiagramMaxLineLabelsOption.DefaultValue, bool isStatic = false)
    {
        var result = default(Diagram);

        var diagramTitle = GetDiagramTitle(activity, levelOfDetails, diagramType);
        if (diagramTitle != null)
        {
            var process = new BusinessProcess(
                            new Activity[] { activity });

            switch (levelOfDetails)
            {
                case DiagramLevelsOfDetailsOption.COMPONENT:
                    {
                        result = new ComponentDiagram(
                            diagramTitle,
                            process: process,
                            showBoundaries: showBoundaries,
                            showInterfaceInputAndOutput: showInterfaceInputAndOutput,
                            maxLineLabels: maxLineLabels,
                            isStatic: isStatic).Build();
                        break;
                    }
                case DiagramLevelsOfDetailsOption.CONTAINER:
                    {
                        result = new ContainerDiagram(
                            diagramTitle,
                            process: process,
                            showBoundaries: showBoundaries,
                            maxLineLabels: maxLineLabels,
                            isStatic: isStatic).Build();
                        break;
                    }
                case DiagramLevelsOfDetailsOption.CONTEXT:
                    {
                        result = new ContextDiagram(
                            diagramTitle,
                            process: process,
                            maxLineLabels: maxLineLabels,
                            isStatic: isStatic).Build();
                        break;
                    }
                default:
                    break;
            }
        }
        else
        {
            Log.Warning("Could not generate diagram title for activity {Name}", activity.Label);
        }

        return result;
    }

    private static Diagram GetDiagram(
        string scope, 
        string levelOfDetails, 
        IEnumerable<Interface> interfaces, 
        bool showBoundaries, 
        bool showInterfaceInputAndOutput, 
        int maxLineLabels = DiagramMaxLineLabelsOption.DefaultValue,
        bool isStatic = false, 
        string? scopedStructureAlias = null)
    {
        var result = default(Diagram);

        var diagramType = isStatic ? DiagramTypesOption.C4_STATIC : DiagramTypesOption.C4;

        var scopedStructureTitle = string.Empty;
        if(scopedStructureAlias != null)
        {
            if(IsNamespaceAlias(scopedStructureAlias))
            {
                scopedStructureTitle = GetTitle(scopedStructureAlias);
            }
            else if (TryParseStructureAlias(scopedStructureAlias, out var system, out var container, out var component, out var @interface))
            {
                if(C4InterFlow.Utils.TryGetNamespaceAlias(scopedStructureAlias, out var namespaceAlias))
                {
                    scopedStructureTitle = GetTitle(namespaceAlias);
                }
                scopedStructureTitle = $"{scopedStructureTitle}{(!string.IsNullOrEmpty(scopedStructureTitle) && system != null ? $" - {system.Label}" : string.Empty)}";
                scopedStructureTitle = $"{scopedStructureTitle}{(!string.IsNullOrEmpty(scopedStructureTitle) && container != null ? $" - {container.Label}" : string.Empty)}";
                scopedStructureTitle = $"{scopedStructureTitle}{(!string.IsNullOrEmpty(scopedStructureTitle) && component != null ? $" - {component.Label}" : string.Empty)}";
                scopedStructureTitle = $"{scopedStructureTitle}{(!string.IsNullOrEmpty(scopedStructureTitle) && @interface != null ? $" - {@interface.Label}" : string.Empty)}";
            }
        }

        var diagramTitle = $"{(!string.IsNullOrEmpty(scopedStructureTitle) ? scopedStructureTitle : ToPrettyName(scope))} - {ToPrettyName(diagramType)} - {ToPrettyName(levelOfDetails)} level";

        var flow = new Flow();

        foreach (var @interface in interfaces)
        {
            flow.Use(@interface.Alias);
        }

        var process = new BusinessProcess(
                        new Activity[] {
                            new Activity(flow, SoftwareSystems.ExternalSystem.ALIAS)
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
                        maxLineLabels: maxLineLabels,
                        isStatic: isStatic).Build();
                    break;
                }
            case DiagramLevelsOfDetailsOption.CONTAINER:
                {
                    result = new ContainerDiagram(
                        diagramTitle,
                        process: process,
                        showBoundaries: showBoundaries,
                        maxLineLabels: maxLineLabels,
                        isStatic: isStatic).Build();
                    break;
                }
            case DiagramLevelsOfDetailsOption.CONTEXT:
                {
                    result = new ContextDiagram(
                        diagramTitle,
                        process: process,
                        maxLineLabels: maxLineLabels,
                        isStatic: isStatic).Build();
                    break;
                }
            default:
                break;
        }

        return result;
    }

    private static Diagram GetDiagram(string diagramType, string levelOfDetails, Interface @interface, bool showBoundaries, bool showInterfaceInputAndOutput, int maxLineLabels = DiagramMaxLineLabelsOption.DefaultValue, bool isStatic = false)
    {
        var result = default(Diagram);

        var diagramTitle = GetDiagramTitle(@interface, levelOfDetails, diagramType);
        if (diagramTitle != null)
        {
            var process = new BusinessProcess(
                            new Activity[] {
                            new Activity(
                                new Flow().Use(@interface.Alias),
                                SoftwareSystems.ExternalSystem.ALIAS)
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
                            maxLineLabels: maxLineLabels,
                            isStatic: isStatic).Build();
                        break;
                    }
                case DiagramLevelsOfDetailsOption.CONTAINER:
                    {
                        result = new ContainerDiagram(
                            diagramTitle,
                            process: process,
                            showBoundaries: showBoundaries,
                            maxLineLabels: maxLineLabels,
                            isStatic: isStatic).Build();
                        break;
                    }
                case DiagramLevelsOfDetailsOption.CONTEXT:
                    {
                        result = new ContextDiagram(
                            diagramTitle,
                            process: process,
                            maxLineLabels: maxLineLabels,
                            isStatic: isStatic).Build();
                        break;
                    }
                default:
                    break;
            }
        }
        else
        {
            Log.Warning("Could not generate diagram title for interface with alias {Alias}", @interface.Alias);
        }


        return result;
    }

    private static void DrawSequenceDiagrams(
        string scope, 
        string levelOfDetails, 
        BusinessProcess[] businessProcesses, 
        string[] formats, 
        bool showBoundaries, 
        bool showInterfaceInputAndOutput, 
        string outputDirectory, 
        string? outputSubDirectory = null,
        string? subtractPath = null,
        string? diagramNamePrefix = null, 
        SequenceDiagramStyle? style = SequenceDiagramStyle.PlantUML)
    {
        if (!businessProcesses.Any()) return;

        var context = new PlantumlSequenceContext(style!.Value);
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

        var diagramType = (style!.Value == SequenceDiagramStyle.C4 ? DiagramTypesOption.C4_SEQUENCE : DiagramTypesOption.SEQUENCE);
        var progress = new ConcurrentProgress(businessProcesses.Count());

        Parallel.ForEach(businessProcesses, businessProcess =>
        {
            var diagram = GetDiagram(
                diagramType, 
                levelOfDetails, 
                businessProcess, 
                showBoundaries, 
                showInterfaceInputAndOutput);

            if (TryGetDiagramPath(
                    scope,
                    levelOfDetails,
                    diagramType,
                    businessProcess,
                    out var path,
                    out var fileName,
                    outputSubDirectory,
                    subtractPath,
                    diagramNamePrefix))
            {
                context.Export(outputDirectory, diagram, path, fileName);
            }
            progress.Increment();
        });
        progress.Complete();
        
    }

    private static void DrawSequenceDiagrams(
        string scope,
        string levelOfDetails,
        Activity[] activities,
        string[] formats,
        bool showBoundaries,
        bool showInterfaceInputAndOutput,
        string outputDirectory,
        string? outputSubDirectory = null,
        string? subtractPath = null,
        string? diagramNamePrefix = null,
        SequenceDiagramStyle? style = SequenceDiagramStyle.PlantUML)
    {
        if (!activities.Any()) return;

        var context = new PlantumlSequenceContext(style!.Value);
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

        var diagramType = (style!.Value == SequenceDiagramStyle.C4 ? DiagramTypesOption.C4_SEQUENCE : DiagramTypesOption.SEQUENCE);
        var progress = new ConcurrentProgress(activities.Count());

        Parallel.ForEach(activities, activity =>
        {
            var diagram = GetDiagram(
                diagramType,
                levelOfDetails,
                activity,
                showBoundaries,
                showInterfaceInputAndOutput);

            if (TryGetDiagramPath(
                    scope,
                    levelOfDetails,
                    diagramType,
                    activity,
                    out var path,
                    out var fileName,
                    outputSubDirectory,
                    subtractPath,
                    diagramNamePrefix))
            {
                context.Export(outputDirectory, diagram, path, fileName);
            }
            progress.Increment();
        });
        progress.Complete();

    }

    private static void DrawC4Diagrams(
        string scope, 
        string levelOfDetails, 
        IEnumerable<BusinessProcess> activities, 
        IEnumerable<string> formats, 
        bool showBoundaries, 
        bool showInterfaceInputAndOutput, 
        int maxLineLabels, 
        string outputDirectory, 
        string? outputSubDirectory = null,
        string? subtractPath = null,
        bool isStatic = false, 
        string? diagramNamePrefix = null)
    {
        if (!activities.Any()) return;

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

        var diagramType = isStatic ? DiagramTypesOption.C4_STATIC : DiagramTypesOption.C4;
        var progress = new ConcurrentProgress(activities.Count());

        Parallel.ForEach(activities, businessProcess =>
        {
            var diagram = GetDiagram(
                diagramType,
                levelOfDetails,
                businessProcess,
                showBoundaries,
                showInterfaceInputAndOutput,
                maxLineLabels,
                isStatic);

            if (TryGetDiagramPath(
                    scope,
                    levelOfDetails,
                    diagramType,
                    businessProcess,
                    out var path,
                    out var fileName,
                    outputSubDirectory,
                    subtractPath,
                    diagramNamePrefix))
            {
                context.Export(outputDirectory, diagram, path, fileName);
            }
            progress.Increment();
        });
        progress.Complete();
    }

    private static void DrawC4Diagrams(
        string scope,
        string levelOfDetails,
        IEnumerable<Activity> activities,
        IEnumerable<string> formats,
        bool showBoundaries,
        bool showInterfaceInputAndOutput,
        int maxLineLabels,
        string outputDirectory,
        string? outputSubDirectory = null,
        string? subtractPath = null,
        bool isStatic = false,
        string? diagramNamePrefix = null)
    {
        if (!activities.Any()) return;

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

        var diagramType = isStatic ? DiagramTypesOption.C4_STATIC : DiagramTypesOption.C4;
        var progress = new ConcurrentProgress(activities.Count());

        Parallel.ForEach(activities, activity =>
        {
            var diagram = GetDiagram(
                diagramType,
                levelOfDetails,
                activity,
                showBoundaries,
                showInterfaceInputAndOutput,
                maxLineLabels,
                isStatic);

            if (TryGetDiagramPath(
                    scope,
                    levelOfDetails,
                    diagramType,
                    activity,
                    out var path,
                    out var fileName,
                    outputSubDirectory,
                    subtractPath,
                    diagramNamePrefix))
            {
                context.Export(outputDirectory, diagram, path, fileName);
            }
            progress.Increment();
        });
        progress.Complete();
    }

    private static void DrawSequenceDiagrams(
        string scope, 
        string levelOfDetails, 
        IEnumerable<Interface> interfaces, 
        string[] formats, 
        bool showBoundaries, 
        bool showInterfaceInputAndOutput, 
        string outputDirectory, 
        string? outputSubDirectory = null, 
        string? subtractPath = null,
        string? diagramNamePrefix = null, 
        SequenceDiagramStyle? style = SequenceDiagramStyle.PlantUML)
    {
        if (!interfaces.Any()) return;

        var context = new PlantumlSequenceContext(style!.Value);
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

        var diagramType = (style!.Value == SequenceDiagramStyle.C4 ? DiagramTypesOption.C4_SEQUENCE : DiagramTypesOption.SEQUENCE);
        var progress = new ConcurrentProgress(interfaces.Count());

        Parallel.ForEach(interfaces, @interface =>
        {
            var diagram = GetDiagram(
                diagramType,
                levelOfDetails,
                @interface,
                showBoundaries,
                showInterfaceInputAndOutput);

            if (TryGetDiagramPath(
                    scope,
                    levelOfDetails,
                    diagramType,
                    @interface,
                    out var path,
                    out var fileName,
                    outputSubDirectory,
                    subtractPath,
                    diagramNamePrefix))
            {
                context.Export(outputDirectory, diagram, path, fileName);
            }
            progress.Increment();
        });
        progress.Complete();
    }

    private static void DrawC4Diagrams(
        string scope, 
        string levelOfDetails, 
        IEnumerable<Interface> interfaces, 
        string[] namespaces, 
        string[] formats, 
        bool showBoundaries, 
        bool showInterfaceInputAndOutput, 
        int maxLineLabels, 
        string outputDirectory, 
        string? outputSubDirectory = null,
        string? subtractPath = null,
        bool isStatic = false, string? 
        diagramNamePrefix = null, 
        bool expandUpstream = false)
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

        if (scope == DiagramScopesOption.ALL_SOFTWARE_SYSTEMS)
        {
            var diagram = GetDiagram(
                scope, 
                levelOfDetails, 
                interfaces, 
                showBoundaries, 
                showInterfaceInputAndOutput, 
                maxLineLabels, 
                isStatic);

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
        else if (scope == DiagramScopesOption.NAMESPACE ||
            scope == DiagramScopesOption.NAMESPACE_SOFTWARE_SYSTEMS)
        {
            string pattern = @"^(.*?)(?:\.SoftwareSystems)";

            var namespaceAliases = new List<string>();

            interfaces.Select(x => Regex.Match(x.Alias, pattern))
                  .Where(m => m.Success)
                  .Select(m => m.Groups[1].Value)
                  .Distinct()
                  .ToList()
                  .ForEach(x =>
                  {
                      var nameSpaceAlias = string.Empty;
                      var segments = x.Split('.');
                      foreach(var segment in segments)
                      {
                          nameSpaceAlias = string.IsNullOrEmpty(nameSpaceAlias) ? segment : $"{nameSpaceAlias}.{segment}";

                          if(!namespaceAliases.Contains(nameSpaceAlias))
                          {
                              namespaceAliases.Add(nameSpaceAlias);
                          }
                      }
                  });

            var progress = new ConcurrentProgress(namespaceAliases.Count());

            foreach(var namespaceAlias in namespaceAliases)
            {
                if (namespaces != null && namespaces.Any() && !namespaces.Contains(namespaceAlias))
                    continue;

                var namespaceInterfaces = scope == DiagramScopesOption.NAMESPACE ?
                    interfaces.Where(x => x.Alias.StartsWith($"{namespaceAlias}.")).ToArray() :
                    interfaces.Where(x => x.Alias.StartsWith($"{namespaceAlias}.SoftwareSystems.")).ToArray();

                if (namespaceInterfaces.Any())
                {
                    var diagram = GetDiagram(
                        scope,
                        levelOfDetails,
                        namespaceInterfaces,
                        showBoundaries,
                        showInterfaceInputAndOutput,
                        maxLineLabels,
                        isStatic,
                        namespaceAlias);

                    if (TryGetDiagramPath(
                            scope,
                            levelOfDetails,
                            isStatic ? DiagramTypesOption.C4_STATIC : DiagramTypesOption.C4,
                            namespaceAlias,
                            out var path,
                            out var fileName,
                            outputSubDirectory,
                            subtractPath,
                            diagramNamePrefix))
                    {
                        context.Export(outputDirectory, diagram, path, fileName);
                    }
                }

                progress.Increment();

            }

            progress.Complete();

        }
        else if (scope == DiagramScopesOption.SOFTWARE_SYSTEM)
        {
            string pattern = @"^(.*?)(?:\.Interfaces|\.Containers)";

            var softwareSystemAliases = interfaces.Select(x => Regex.Match(x.Alias, pattern))
                  .Where(m => m.Success)
                  .Select(m => m.Groups[1].Value)
                  .Distinct();

            var progress = new ConcurrentProgress(softwareSystemAliases.Count());

            Parallel.ForEach(softwareSystemAliases, softwareSystemAlias =>
            {
                var systemInterfaces = interfaces.Where(x => x.Alias.StartsWith($"{softwareSystemAlias}.")).ToArray();
                var upstreamInterfaces = GetUpstreamInterfaces(systemInterfaces, expandUpstream);

                var diagram = GetDiagram(
                    scope,
                    levelOfDetails,
                    expandUpstream && upstreamInterfaces.Any() ? upstreamInterfaces : systemInterfaces,
                    showBoundaries,
                    showInterfaceInputAndOutput,
                    maxLineLabels,
                    isStatic,
                    softwareSystemAlias);

                if (TryGetDiagramPath(
                        scope,
                        levelOfDetails,
                        isStatic ? DiagramTypesOption.C4_STATIC : DiagramTypesOption.C4,
                        systemInterfaces.First(),
                        out var path,
                        out var fileName,
                        outputSubDirectory,
                        subtractPath,
                        diagramNamePrefix))
                {
                    context.Export(outputDirectory, diagram, path, fileName);
                }

                progress.Increment();

            });

            progress.Complete();
        }
        else if (scope == DiagramScopesOption.CONTAINER)
        {
            string pattern = @"(.*?\.Containers\.[^.]+)";
            var containerAliases = interfaces.Select(x => Regex.Match(x.Alias, pattern))
                  .Where(m => m.Success)
                  .Select(m => m.Groups[1].Value)
                  .Distinct();

            var progress = new ConcurrentProgress(containerAliases.Count());

            Parallel.ForEach(containerAliases, containerAlias =>
            {
                var containerInterfaces = interfaces.Where(x => x.Alias.StartsWith($"{containerAlias}.")).ToArray();
                var upstreamInterfaces = GetUpstreamInterfaces(containerInterfaces, expandUpstream);
                
                var diagram = GetDiagram(
                    scope,
                    levelOfDetails,
                    expandUpstream && upstreamInterfaces.Any() ? upstreamInterfaces : containerInterfaces,
                    showBoundaries,
                    showInterfaceInputAndOutput,
                    maxLineLabels,
                    isStatic,
                    containerAlias);

                if (TryGetDiagramPath(
                        scope,
                        levelOfDetails,
                        isStatic ? DiagramTypesOption.C4_STATIC : DiagramTypesOption.C4,
                        containerInterfaces.First(),
                        out var path,
                        out var fileName,
                        outputSubDirectory,
                        subtractPath,
                        diagramNamePrefix))
                {
                    context.Export(outputDirectory, diagram, path, fileName);
                }

                progress.Increment();

            });

            progress.Complete();
        }
        else if (scope == DiagramScopesOption.COMPONENT)
        {
            string pattern = @"(.*?\.Components\.[^.]+)";
            var componentAliases = interfaces.Select(x => Regex.Match(x.Alias, pattern))
                  .Where(m => m.Success)
                  .Select(m => m.Groups[1].Value)
                  .Distinct();

            var progress = new ConcurrentProgress(componentAliases.Count());

            Parallel.ForEach(componentAliases, componentAlias =>
            {
                var componentInterfaces = interfaces.Where(x => x.Alias.StartsWith($"{componentAlias}.")).ToArray();
                var upstreamInterfaces = GetUpstreamInterfaces(componentInterfaces, expandUpstream);

                var diagram = GetDiagram(
                    scope,
                    levelOfDetails,
                    expandUpstream && upstreamInterfaces.Any() ? upstreamInterfaces : componentInterfaces,
                    showBoundaries,
                    showInterfaceInputAndOutput,
                    maxLineLabels,
                    isStatic,
                    componentAlias);

                if (TryGetDiagramPath(
                        scope,
                        levelOfDetails,
                        isStatic ? DiagramTypesOption.C4_STATIC : DiagramTypesOption.C4,
                        componentInterfaces.First(),
                        out var path,
                        out var fileName,
                        outputSubDirectory,
                        subtractPath,
                        diagramNamePrefix))
                {
                    context.Export(outputDirectory, diagram, path, fileName);
                }

                progress.Increment();

            });

            progress.Complete();
        }
        else
        {
            var diagramType = isStatic ? DiagramTypesOption.C4_STATIC : DiagramTypesOption.C4;
            var progress = new ConcurrentProgress(interfaces.Count());

            Parallel.ForEach(interfaces, @interface =>
            {
                var upstreamInterfaces = GetUpstreamInterfaces(new Interface[] { @interface }, expandUpstream);

                var diagram = expandUpstream && upstreamInterfaces.Any() ?
                    GetDiagram(
                        diagramType,
                        levelOfDetails,
                        upstreamInterfaces,
                        showBoundaries,
                        showInterfaceInputAndOutput,
                        maxLineLabels,
                        isStatic,
                        @interface.Alias) : 
                    GetDiagram(
                        diagramType,
                        levelOfDetails,
                        @interface,
                        showBoundaries,
                        showInterfaceInputAndOutput,
                        maxLineLabels,
                        isStatic);

                if (TryGetDiagramPath(
                    scope,
                    levelOfDetails,
                    diagramType,
                    @interface,
                    out var path,
                    out var fileName,
                    outputSubDirectory,
                    subtractPath,
                    diagramNamePrefix))
                {
                    context.Export(outputDirectory, diagram, path, fileName);
                }
                progress.Increment();
            });
            progress.Complete();
        }
    }

    private static IEnumerable<Interface> GetUpstreamInterfaces(IEnumerable<Interface> interfaces, bool expandUpstream)
    {
        var result = new List<Interface>();

        if (expandUpstream)
        {
            var queryOutputContextKey = QueryUseFlowsCommand.OUTPUT_CONTEXT_KEY;
            var commandResult = QueryUseFlowsCommand.Execute(
                interfaces.Select(x => x.Alias).ToArray(), true,
                outputToContext: true).Result;

            if (Context.Instance.TryGetValue(queryOutputContextKey, out var value))
            {
                if (value is IEnumerable<string> contextInterfaces)
                {
                    result = GetInterfaces(contextInterfaces.Distinct(), DiagramScopesOption.ALL_SOFTWARE_SYSTEMS).Result.ToList();
                }
            }
        }
        return result;
    }
    private static bool TryGetDiagramPath(
        string scope, 
        string levelOfDetails, 
        string diagramType, 
        out string? path, 
        out string? fileName, 
        string? outputSubDirectory = null, 
        string? diagramNamePrefix = null)
    {
        if(scope == DiagramScopesOption.ALL_SOFTWARE_SYSTEMS)
        {
            path = !string.IsNullOrEmpty(outputSubDirectory) ? outputSubDirectory : string.Empty;
            fileName = $"{(!string.IsNullOrEmpty(diagramNamePrefix) ? $"{diagramNamePrefix} - " : string.Empty)}{ToPrettyName(levelOfDetails)} - {ToPrettyName(diagramType)}.puml";
        }
        else
        {
            path = fileName = null;
        }
            
        return path != null && !string.IsNullOrEmpty(fileName);
    }

    private static bool TryGetDiagramPath(
        string scope,
        string levelOfDetails,
        string diagramType,
        string namespaceAlias,
        out string path,
        out string fileName,
        string? outputSubDirectory = null,
        string? subtractPath = null,
        string? diagramNamePrefix = null)
        
    {
        if(scope != DiagramScopesOption.NAMESPACE &&
            scope != DiagramScopesOption.NAMESPACE_SOFTWARE_SYSTEMS)
        {
            path = fileName = null;
            return false;
        }

        path = C4InterFlow.Utils.GetPathFromAlias(namespaceAlias);

        if (!string.IsNullOrEmpty(subtractPath))
        {
            path = path.Replace(subtractPath, string.Empty);
        }

        path = Path.Join(!string.IsNullOrEmpty(outputSubDirectory) ? outputSubDirectory : string.Empty, path);

        if (scope == DiagramScopesOption.NAMESPACE_SOFTWARE_SYSTEMS)
        {
            path = Path.Join(path, "Software Systems");
        }

        fileName = $"{(!string.IsNullOrEmpty(diagramNamePrefix) ? $"{diagramNamePrefix} - " : string.Empty)}{ToPrettyName(levelOfDetails)} - {ToPrettyName(diagramType)}.puml";

        return !string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(fileName);
    }
    private static bool TryGetDiagramPath(
        string scope, 
        string levelOfDetails, 
        string diagramType, 
        Interface @interface, 
        out string path, 
        out string fileName, 
        string? outputSubDirectory = null,
        string? subtractPath = null,
        string? diagramNamePrefix = null)
    {

        path = C4InterFlow.Utils.TryGetNamespaceAlias(@interface.Alias, out var namespaceAlias) ?
                C4InterFlow.Utils.GetPathFromAlias(namespaceAlias) : string.Empty;

        if(!string.IsNullOrEmpty(subtractPath))
        {
            path = path.Replace(subtractPath, string.Empty);
        }

        path = Path.Join(!string.IsNullOrEmpty(outputSubDirectory) ? outputSubDirectory : string.Empty, path);

        fileName = $"{(!string.IsNullOrEmpty(diagramNamePrefix) ? $"{diagramNamePrefix} - " : string.Empty)}{ToPrettyName(levelOfDetails)} - {ToPrettyName(diagramType)}.puml";


        if(scope == DiagramScopesOption.ALL_SOFTWARE_SYSTEMS ||
            scope == DiagramScopesOption.NAMESPACE)
        {
            return !string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(fileName);
        }

        path = Path.Join(path, "Software Systems");

        switch (scope)
        {
            case DiagramScopesOption.SOFTWARE_SYSTEM:
                {
                    if (!TryParseInterface(@interface, out var softwareSystem))
                    {
                        Log.Warning("Could not get Software System for interface with {Alias} alias", @interface.Alias);

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
                        Log.Warning("Could not get Software System for interface with {Alias} alias", @interface.Alias);

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
                       Log.Warning("Could not get {Type} for interface with {Alias} alias", softwareSystem == null ? "Software System" : "Container", @interface.Alias);
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
                        Log.Warning("Could not get {Type} for interface with {Alias} alias", softwareSystem == null ? "Software System" : "Container", @interface.Alias);
                        
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
                        Log.Warning("Could not get {Type} for interface with {Alias} alias", softwareSystem == null ? "Software System" : container == null ? "Container" : "Component", @interface.Alias);

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
                        Log.Warning("Could not get {Type} for interface with {Alias} alias", softwareSystem == null ? "Software System" : container == null ? "Container" : "Component", @interface.Alias);
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

    private static bool TryGetDiagramPath(
        string scope,
        string levelOfDetails,
        string diagramType,
        Activity activity,
        out string path,
        out string fileName,
        string? outputSubDirectory = null,
        string? subtractPath = null,
        string? diagramNamePrefix = null)
    {

        path = C4InterFlow.Utils.TryGetNamespaceAlias(activity.Alias, out var namespaceAlias) ?
            C4InterFlow.Utils.GetPathFromAlias(namespaceAlias) : string.Empty;

        if (!string.IsNullOrEmpty(subtractPath))
        {
            path = path.Replace(subtractPath, string.Empty);
        }

        path = Path.Join(!string.IsNullOrEmpty(outputSubDirectory) ? outputSubDirectory : string.Empty, path);

        path = Path.Join(path, "Activities");

        fileName = $"{(!string.IsNullOrEmpty(diagramNamePrefix) ? $"{diagramNamePrefix} - " : string.Empty)}{ToPrettyName(levelOfDetails)} - {ToPrettyName(diagramType)}.puml";

        switch (scope)
        {
            case DiagramScopesOption.ACTIVITY:
                {
                    if(TryParseActivityAlias(activity.Alias, out var actor))
                    {
                        path = Path.Join(path, actor.Label, activity.Label);  
                    }
                    else
                    {
                        path = fileName = null;
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

    private static bool TryGetDiagramPath(
        string scope, 
        string levelOfDetails, 
        string diagramType, 
        BusinessProcess businessProcess, 
        out string path, 
        out string fileName, 
        string? outputSubDirectory = null,
        string? subtractPath = null,
        string? diagramNamePrefix = null)
    {
        
        path = C4InterFlow.Utils.TryGetNamespaceAlias(businessProcess.Alias, out var namespaceAlias) ?
            C4InterFlow.Utils.GetPathFromAlias(namespaceAlias) : string.Empty;

        if (!string.IsNullOrEmpty(subtractPath))
        {
            path = path.Replace(subtractPath, string.Empty);
        }

        path = Path.Join(!string.IsNullOrEmpty(outputSubDirectory) ? outputSubDirectory : string.Empty, path);

        path = Path.Join(path, "Business Processes");

        fileName = $"{(!string.IsNullOrEmpty(diagramNamePrefix) ? $"{diagramNamePrefix} - " : string.Empty)}{ToPrettyName(levelOfDetails)} - {ToPrettyName(diagramType)}.puml";

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

    private static string? GetDiagramTitle(BusinessProcess? businessProcess, string levelOfDetails, string diagramType)
    {
        if (string.IsNullOrEmpty(businessProcess?.Label))
            return null;

        
        if (C4InterFlow.Utils.TryGetNamespaceAlias(businessProcess.Alias, out var namespaceAlias))
            return $"{GetTitle(namespaceAlias)} - {businessProcess.Label} - {ToPrettyName(diagramType)} - {ToPrettyName(levelOfDetails)} level";
        else
            return null;
    }

    private static string? GetDiagramTitle(Activity? activity, string levelOfDetails, string diagramType)
    {
        if (string.IsNullOrEmpty(activity?.Label))
            return null;


        if (C4InterFlow.Utils.TryGetNamespaceAlias(activity.Alias, out var namespaceAlias) &&
            TryParseActivityAlias(activity.Alias, out var actor))
            return $"{GetTitle(namespaceAlias)} - {actor.Label} - {activity.Label} - {ToPrettyName(diagramType)} - {ToPrettyName(levelOfDetails)} level";
        else
            return null;
    }

    private static string? GetDiagramTitle(Interface? @interface, string levelOfDetails, string diagramType)
    {
        if (string.IsNullOrEmpty(@interface?.Alias))
            return null;

        if (C4InterFlow.Utils.TryGetNamespaceAlias(@interface.Alias, out var namespaceAlias) &&
            TryParseInterface(@interface, out var system, out var container, out var component))
        {
            return $"{GetTitle(namespaceAlias)} - {system.Label}{(container != null ? $" - {container.Label}" : string.Empty)}{(component != null ? $" - {component.Label}" : string.Empty)} - {@interface.Label} - {ToPrettyName(diagramType)} - {ToPrettyName(levelOfDetails)} level";
        }

        return null;
    }

    private static string GetTitle(string? alias)
    {
        var result = string.Empty;

        if (alias == null)
            return result;

        foreach (var segment in alias.Split('.'))
        {
            var segmentSubTitle = C4InterFlow.Utils.GetLabel(segment);
            result = $"{(string.IsNullOrEmpty(result) ? $"{segmentSubTitle}" : $"{result} - {segmentSubTitle}")}";
        }

        return result;
    }

    private static bool TryParseInterface(Interface @interface, out SoftwareSystem? system, out Container? container, out Component? component)
    {
        TryParseInterfaceAlias(@interface.Alias, out system, out container, out component);

        return (system as Structure ?? container as Structure ?? component as Structure) != null;
    }

    private static bool TryParseInterface(Interface @interface, out SoftwareSystem? system, out Container? container)
    {
        TryParseInterfaceAlias(@interface.Alias, out system, out container, out var tempComponent);

        return (system as Structure ?? container as Structure) != null;
    }

    private static bool TryParseInterface(Interface @interface, out SoftwareSystem? system)
    {
        TryParseInterfaceAlias(@interface.Alias, out system, out var tempContainer, out var tempComponent);

        return (system as Structure) != null;
    }

    private static bool TryParseStructureAlias(string structureAlias, out SoftwareSystem? system, out Container? container, out Component? component, out Interface? @interface)
    {
        container = default(Container);
        system = default(SoftwareSystem);
        component = default(Component);
        @interface = default(Interface);

        if (structureAlias == null)
            return false;

        try
        {
            if (structureAlias.Contains(".Interfaces."))
            {
                @interface = C4InterFlow.Utils.GetInstance<Interface>(structureAlias);
            }
            
            if (structureAlias.Contains(".Components."))
            {
                component = C4InterFlow.Utils.GetInstance<Component>(@interface == null ? structureAlias : @interface.Owner);
                container = C4InterFlow.Utils.GetInstance<Container>(component?.Container);
                system = C4InterFlow.Utils.GetInstance<SoftwareSystem>(container?.SoftwareSystem);
            }
            else if (structureAlias.Contains(".Containers."))
            {
                container = C4InterFlow.Utils.GetInstance<Container>(@interface == null ? structureAlias : @interface.Owner);
                system = C4InterFlow.Utils.GetInstance<SoftwareSystem>(container?.SoftwareSystem);
            }
            else if ((structureAlias.Contains(".SoftwareSystems.")))
            {
                system = C4InterFlow.Utils.GetInstance<SoftwareSystem>(@interface == null ? structureAlias : @interface.Owner);
            }

            return (system as Structure ?? container as Structure ?? component as Structure ?? @interface as Structure) != null;
        }
        catch
        {
            return false;
        }

    }

    private static bool IsNamespaceAlias(string alias)
    {
        return !alias.Contains(".BusinessProcesses") &&
            !alias.Contains(".SoftwareSystems") &&
            !alias.Contains(".Actors");
    }

    private static bool TryParseInterfaceAlias(string interfaceAlias, out SoftwareSystem? system, out Container? container, out Component? component)
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

    private static bool TryParseActivityAlias(string activityAlias, out Structure? actor)
    {
        actor = default;

        var activitiesSegment = ".Activities.";
        if (activityAlias == null || !activityAlias.Contains(activitiesSegment)) return false;

        try
        {
            var actorAlias = activityAlias.Substring(0, activityAlias.IndexOf(activitiesSegment));
            actor = C4InterFlow.Utils.GetInstance<Structure>(actorAlias);

            return (actor) != null;
        }
        catch
        {
            return false;
        }

    }
}
