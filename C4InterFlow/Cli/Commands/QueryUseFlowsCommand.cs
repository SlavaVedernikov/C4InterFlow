using System.CommandLine;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;
using C4InterFlow.Cli.Commands.Options;
using System.Reflection;
using C4InterFlow.Automation;
using C4InterFlow.Cli.Commands.Binders;
using C4InterFlow.Commons.Extensions;
using Serilog;
using Serilog.Events;
using static C4InterFlow.SoftwareSystems.ExternalSystem;

namespace C4InterFlow.Cli.Commands;

public class QueryUseFlowsCommand : Command
{
    private const string COMMAND_NAME = "query-use-flows";
    public const string OUTPUT_CONTEXT_KEY = $"{nameof(QueryUseFlowsCommand)}Result";
    public QueryUseFlowsCommand() : base(COMMAND_NAME,
        "Finds all interface(s) that Use given interface(s) in their Flows.")
    {
        var interfacesOption = InterfacesOption.Get();
        var isRecursiveOption = QueryIsRecursiveOption.Get();
        var queryOutputFileOption = QueryOutputFileOption.Get();
        var queryAppendOption = QueryAppendOption.Get();
        var architectureAsCodeInputPathsOption = AaCInputPathsOption.Get();
        var architectureAsCodeReaderStrategyTypeOption = AaCReaderStrategyTypeOption.Get();
        
        AddOption(interfacesOption);
        AddOption(isRecursiveOption);
        AddOption(queryOutputFileOption);
        AddOption(queryAppendOption);
        AddOption(architectureAsCodeInputPathsOption);
        AddOption(architectureAsCodeReaderStrategyTypeOption);
        

        this.SetHandler(async (interfaceAliases, isRecursive, architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType, queryOutputFile, append) =>
        {
            if (!AaCReaderContext.HasStrategy)
            {
                Utils.SetArchitectureAsCodeReaderContext(architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType);
            }

            await Execute(interfaceAliases, isRecursive, queryOutputFile, append);
        },
        interfacesOption, isRecursiveOption, architectureAsCodeInputPathsOption, architectureAsCodeReaderStrategyTypeOption, queryOutputFileOption, queryAppendOption);
    }

    public static async Task<int> Execute(string[] interfaceAliases, bool isRecursive, string queryOutputFile = null, bool append = false, bool outputToContext = false)
    {
        try
        {
            Log.Information("{Name} command is executing", COMMAND_NAME);

            var resolvedInterfaceAliases = Utils.ResolveStructures(interfaceAliases);
            var result = new List<string>();
            var interfaces = Utils.GetAllInterfaces();

            foreach (var interfaceAlias in resolvedInterfaceAliases)
            {
                result.AddRange(GetUsedBy(interfaces, interfaceAlias, isRecursive));
            }

            result = result.Distinct().ToList();

            var outputToFile = !string.IsNullOrEmpty(queryOutputFile);
            if (outputToFile)
            {
                Utils.WriteLines(result, queryOutputFile, append);

                Log.Information("{Name} command completed. See query results in {File}", COMMAND_NAME, queryOutputFile);
            }
            
            if(outputToContext)
            {
                Context.Instance.Remove(OUTPUT_CONTEXT_KEY);
                Context.Instance.AddOrUpdate(OUTPUT_CONTEXT_KEY, result.Distinct());
            }

            if(!outputToFile && !outputToContext)
            {
                Log.Information("{Name} command completed. Query results: {Results}", COMMAND_NAME, result.Distinct().ToArray());
            }

            return 0;
        }
        catch (Exception e)
        {
            Log.Error(e, "{Name} command failed with exception: {Error}",COMMAND_NAME,e.Message);

            return 1;
        }
    }

    //TODO: Add includePrivateInterfaces parameter (default is false)
    //TODO: Move GetUsedBy into Utils
    //TODO: Add support for queries to DrawDiagramsCommand
    private static IEnumerable<string> GetUsedBy(IEnumerable<Interface> interfaces, string interfaceAlias, bool isRecursive)
    {
        var result = new List<string>();
        var @interface = C4InterFlow.Utils.GetInstance<Interface>(interfaceAlias);

        foreach (var interfaceInstance in interfaces)
        {
            if (interfaceInstance?.Flow?.GetUsesInterfaces()?.Select(x => x.Alias)?.Contains(interfaceAlias) == true)
            {
                result.Add(interfaceInstance.Alias);
            }
        }

        if (isRecursive)
        {
            var tempAliases = result.ToArray();

            foreach (var alias in tempAliases)
            {
                var tempResult = GetUsedBy(interfaces, alias, isRecursive);
                if(tempResult.Any())
                {
                    result.Remove(alias);
                    result.AddRange(tempResult);
                }
            }
        }

        return result;
    }
}
