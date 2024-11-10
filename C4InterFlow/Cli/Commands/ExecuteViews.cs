using System.CommandLine;
using C4InterFlow.Structures;
using C4InterFlow.Cli.Commands.Options;
using C4InterFlow.Automation;
using System.Reflection;
using C4InterFlow.Cli.Commands.Binders;
using C4InterFlow.Commons.Extensions;
using Serilog;
using Serilog.Events;
using C4InterFlow.Structures.Interfaces;
using C4InterFlow.Structures.Views;

namespace C4InterFlow.Cli.Commands;

public class ExecuteViewsCommand : Command
{
    private const string COMMAND_NAME = "execute-views";
    public ExecuteViewsCommand() : base(COMMAND_NAME,
        "Executes Architecture As Code generation Strategy")
    {
        var viewsOption = ViewsOption.Get();
        var outputDirectoryOption = OutputDirectoryOption.Get();
        var diagramNamePrefixOption = DiagramNamePrefixOption.Get();
        var architectureAsCodeInputPathsOption = AaCInputPathsOption.Get();
        var architectureAsCodeReaderStrategyTypeOption = AaCReaderStrategyTypeOption.Get();
        var viewsInputPathsOption = ViewsInputPathsOption.Get();

        AddOption(viewsOption);
        AddOption(outputDirectoryOption);
        AddOption(diagramNamePrefixOption);
        AddOption(architectureAsCodeInputPathsOption);
        AddOption(architectureAsCodeReaderStrategyTypeOption);
        AddOption(viewsInputPathsOption);

        this.SetHandler(async (views, outputDirectory, diagramNamePrefix, architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType, viewsInputPaths) =>
            {
                if (!AaCReaderContext.HasStrategy)
                {
                    Utils.SetArchitectureAsCodeReaderContext(architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType, viewsInputPaths);
                }
                await Execute(views, outputDirectory, diagramNamePrefix, viewsInputPaths);
            },
            viewsOption, outputDirectoryOption, diagramNamePrefixOption, architectureAsCodeInputPathsOption, architectureAsCodeReaderStrategyTypeOption, viewsInputPathsOption);
    }

    private static async Task<int> Execute(string[] views, string outputDirectory, string diagramNamePrefix, string[] viewsInputPaths)
    {
        try
        {
            Log.Information("{Name} command is executing", COMMAND_NAME);

            
            AaCReaderContext.Strategy.Validate(out var errors);

            if (errors.Any())
            {
                foreach (var validationError in errors)
                {
                    Log.Error(validationError.Template, validationError.Args);
                }

                throw new InvalidDataException("AaC has errors. Please resolve and retry.");
            }

            var resolvedViewAliases = new List<string>();
            resolvedViewAliases.AddRange(Utils.ResolveStructures(views));

            resolvedViewAliases = resolvedViewAliases.Distinct().ToList();

            var viewInstances = GetViews(resolvedViewAliases);

            foreach ( var view in viewInstances )
            {
                var interfaces = view.Interfaces;

                await DrawDiagramsCommand.Execute(
                    new DiagramOptions(
                        view.Scopes ?? DiagramScopesOption.GetAllValues(), 
                        view.Types ?? DiagramTypesOption.GetAllValues(), 
                        view.LevelsOfDetails ?? DiagramLevelsOfDetailsOption.GetAllValues()),
                    new InputOptions(
                        interfaces,
                        null,
                        view.BusinessProcesses,
                        view.Namespaces),
                    new DisplayOptions(
                        false, 
                        view.MaxLineLabels ?? DiagramMaxLineLabelsOption.DefaultValue,
                        view.ExpandUpstream.HasValue ? view.ExpandUpstream.Value : false),
                    new OutputOptions(
                        outputDirectory,
                        Path.Join("Views",view.Name),
                        diagramNamePrefix, 
                        view.Formats ?? new string[] { }));
            }

            return 0;
        }
        catch (Exception e)
        {
            Log.Error(e, "{Name} command execution failed: {Error}", COMMAND_NAME, e.Message);

            return 1;
        }
    }

    private static IEnumerable<View> GetViews(IEnumerable<string> viewAliases)
    {
        var result = new List<View>();

        foreach (string viewAlias in viewAliases)
        {
            var viewInstance = C4InterFlow.Utils.GetInstance<View>(viewAlias);
            if (viewInstance != null)
            {
                result.Add(viewInstance);
            }
            else
            {
                Log.Warning("Could not load View instance for alias {ViewAlias}", viewAlias);
            }
        }

        return result;
    }

}
