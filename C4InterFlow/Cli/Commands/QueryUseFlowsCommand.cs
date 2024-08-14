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

namespace C4InterFlow.Cli.Commands;

public class QueryUseFlowsCommand : Command
{
    private const string COMMAND_NAME = "query-use-flows";
    public QueryUseFlowsCommand() : base(COMMAND_NAME,
        "Finds all interface(s) that Use given interface(s) in their Flows.")
    {
        var interfacesOption = InterfacesOption.Get();
        var isRecursiveOption = QueryIsRecursiveOption.Get();
        var queryOutputFileOption = QueryOutputFileOption.Get();
        var queryAppendOption = QueryAppendOption.Get();
        var architectureAsCodeInputPathsOption = AaCInputPathsOption.Get();
        var architectureAsCodeReaderStrategyTypeOption = AaCReaderStrategyTypeOption.Get();
        var queryIncludeSelfOption = QueryIncludeSelfOption.Get();
        
        AddOption(interfacesOption);
        AddOption(isRecursiveOption);
        AddOption(queryOutputFileOption);
        AddOption(queryAppendOption);
        AddOption(architectureAsCodeInputPathsOption);
        AddOption(architectureAsCodeReaderStrategyTypeOption);
        AddOption(queryIncludeSelfOption);
  
        this.SetHandler(async (interfaceAliases, isRecursive, queryIncludeSelf, queryOutputFile, append, architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType) =>
        {
            await Execute(interfaceAliases, isRecursive, queryIncludeSelf, queryOutputFile, append, architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType);
        },
        interfacesOption, isRecursiveOption, queryIncludeSelfOption, queryOutputFileOption, queryAppendOption, architectureAsCodeInputPathsOption, architectureAsCodeReaderStrategyTypeOption);
    }

    public static async Task<int> Execute(string[] interfaceAliases, bool isRecursive, bool queryIncludeSelf, string queryOutputFile, bool append, string[] architectureAsCodeInputPaths, string architectureAsCodeReaderStrategyType)
    {
        try
        {
            Log.Information("{Name} command is executing", COMMAND_NAME);

            if (!AaCReaderContext.HasStrategy)
            {
                Utils.SetArchitectureAsCodeReaderContext(architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType);
            }

            var resolvedInterfaceAliases = Utils.ResolveStructures(interfaceAliases);
            var result = new List<string>();
            var interfaces = Utils.GetAllInterfaces();

            foreach (var interfaceAlias in resolvedInterfaceAliases)
            {
                GetUsedBy(interfaces, interfaceAlias, isRecursive, result, queryIncludeSelf);        
            }

            result = result.Distinct().ToList();

            if (!string.IsNullOrEmpty(queryOutputFile))
            {
                Utils.WriteLines(result, queryOutputFile, append);

                Log.Information("{Name} command completed. See query results in {File}", COMMAND_NAME, queryOutputFile);
            }
            else
            {
                Log.Information("{Name} command completed. Query results: {Results}", COMMAND_NAME, result.Distinct().ToArray());
                // Console.WriteLine($"'{COMMAND_NAME}' command completed. See query results below.");
                // Console.Write($"{string.Join(Environment.NewLine, result.Distinct().ToArray())}");
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
    private static IEnumerable<string> GetUsedBy(IEnumerable<Interface> interfaces, string interfaceAlias, bool isRecursive, List<string> usedByResult, bool queryIncludeSelf)
    {
        var result = new List<string>();

        if (C4InterFlow.Utils.GetInstance<Interface>(interfaceAlias)?.IsPrivate == true)
        {
            return result;
        }

        foreach (var interfaceInstance in interfaces)
        {
            if (interfaceInstance?.Flow?.GetUsesInterfaces()?.Select(x => x.Alias)?.Contains(interfaceAlias) == true)
            {
                if (isRecursive)
                {
                    var tempResult = GetUsedBy(interfaces, interfaceInstance.Alias, isRecursive, usedByResult, queryIncludeSelf);
                    if(!tempResult.Any())
                    { 
                        result.Add(interfaceInstance.Alias);
                    }
                }
                else
                {
                    result.Add(interfaceInstance.Alias);
                }

            }
        }

        if(!result.Any() && queryIncludeSelf)
        {
            result.Add(interfaceAlias);
        }

        usedByResult.AddRange(result);
        return result;
    }
}
