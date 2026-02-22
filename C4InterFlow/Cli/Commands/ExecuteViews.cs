using System.CommandLine;
using C4InterFlow.Cli.Commands.Options;
using C4InterFlow.Automation;
using C4InterFlow.Cli.Commands.Binders;
using Serilog;
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
        var viewInputPathsOption = ViewInputPathsOption.Get();

        AddOption(viewsOption);
        AddOption(outputDirectoryOption);
        AddOption(diagramNamePrefixOption);
        AddOption(architectureAsCodeInputPathsOption);
        AddOption(architectureAsCodeReaderStrategyTypeOption);
        AddOption(viewInputPathsOption);

        this.SetHandler(async (views, outputDirectory, diagramNamePrefix, architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType, viewInputPaths) =>
            {
                if (!AaCReaderContext.HasStrategy)
                {
                    Utils.SetArchitectureAsCodeReaderContext(architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType, viewInputPaths);
                }
                await Execute(views, outputDirectory, diagramNamePrefix);
            },
            viewsOption, outputDirectoryOption, diagramNamePrefixOption, architectureAsCodeInputPathsOption, architectureAsCodeReaderStrategyTypeOption, viewInputPathsOption);
    }

    private static async Task<int> Execute(string[] views, string outputDirectory, string diagramNamePrefix)
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

            var viewInstances = GetViews(Utils.ResolveStructures(views)?.Distinct());

            foreach ( var view in viewInstances )
            {
                Log.Information("Executing view {View}.", view.Alias);

                var interfaces = Utils.ResolveStructures(view.Interfaces)?.Distinct();
                if (C4InterFlow.Utils.TryGetNamespaceAlias(view.Alias, out var viewNamespace) && !string.IsNullOrEmpty(viewNamespace))
                {
                    var viewNamespacedInterfaces = interfaces.Where(x => x.StartsWith($"{viewNamespace}."));
                    var numberOfExcludedInterfaces = interfaces.Count() - viewNamespacedInterfaces.Count();
                    if (numberOfExcludedInterfaces > 0)
                    {
                        Log.Warning("{NumberOfExludedInterfaces} interfaces will be excluded from {ViewName} view, as they are located outside of the view namespace {ViewNamespace}.", numberOfExcludedInterfaces, view.Name, viewNamespace);

                        interfaces.Except(viewNamespacedInterfaces).ToList().ForEach(x =>
                        {
                            Log.Debug("{ExludedInterface}", x);
                        });
                        interfaces = viewNamespacedInterfaces;
                    }
                }
                await DrawDiagramsCommand.Execute(
                    new DiagramOptions(
                        view.Scopes ?? DiagramScopesOption.GetAllValues(), 
                        view.Types ?? DiagramTypesOption.GetAllValues(), 
                        view.LevelsOfDetails ?? DiagramLevelsOfDetailsOption.GetAllValues()),
                    new InputOptions(
                        interfaces?.Distinct().ToArray(),
                        null,
                        view.BusinessProcesses,
                        view.Namespaces,
                        view.Activities),
                    new DisplayOptions(
                        false, 
                        view.MaxLineLabels ?? DiagramMaxLineLabelsOption.DefaultValue,
                        view.ExpandUpstream.HasValue ? view.ExpandUpstream.Value : false),
                    new OutputOptions(
                        outputDirectory,
                        diagramNamePrefix, 
                        view.Formats ?? new string[] { },
                        C4InterFlow.Utils.GetPathFromAlias(view.Alias),
                        C4InterFlow.Utils.TryGetNamespaceAlias(view.Alias, out var namespaceAlias) ?
                        C4InterFlow.Utils.GetPathFromAlias(namespaceAlias) : null));
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
