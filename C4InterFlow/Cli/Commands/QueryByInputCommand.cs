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

public class QueryByInputCommand : Command
{
    private const string COMMAND_NAME = "query-by-input";
    public QueryByInputCommand() : base(COMMAND_NAME,
        "Queries interfaces by alias values in Interface 'Input' field.")
    {
        var entitiesOption = EntitiesOption.Get();
        var architectureAsCodeInputPathsOption = AaCInputPathsOption.Get();
        var architectureAsCodeReaderStrategyTypeOption = AaCReaderStrategyTypeOption.Get();
        
        AddOption(entitiesOption);
        AddOption(architectureAsCodeInputPathsOption);
        AddOption(architectureAsCodeReaderStrategyTypeOption);
        
        this.SetHandler(async (entityAliases, architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType) =>
            {
                await Execute(entityAliases, architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType);
            },
            entitiesOption, architectureAsCodeInputPathsOption, architectureAsCodeReaderStrategyTypeOption);
    }

    private static async Task<int> Execute(string[] entityAliases, string[] architectureAsCodeInputPaths, string architectureAsCodeReaderStrategyType)
    {
        try
        {
            Log.Information("{Name} command is executing", COMMAND_NAME);

            if(!AaCReaderContext.HasStrategy)
            {
                Utils.SetArchitectureAsCodeReaderContext(architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType);
            }

            var resolvedEntityAliases = Utils.ResolveStructures(entityAliases);
            var result = new List<string>();
            var interfaces = Utils.GetAllInterfaces();
            
            foreach (var entityAlias in resolvedEntityAliases)
            {
                result.AddRange(GetByInput(interfaces, entityAlias));
            }
            Log.Information("{Name} command completed. Query results: {Results}", COMMAND_NAME, result.Distinct().ToArray());
            return 0;
        }
        catch (Exception e)
        {
            Log.Error(e, "{Name} command failed with execption: {Error}", COMMAND_NAME, e.Message);

            return 1;
        }
    }

    private static IEnumerable<string> GetByInput(IEnumerable<Interface> interfaces, string entityAlias)
    {
        var result = new List<string>();

        foreach (var interfaceInstance in interfaces)
        {
            if (interfaceInstance?.Input?.Equals(entityAlias) == true)
            {  
                result.Add(interfaceInstance.Alias);
            }
        }

        return result;
    }
}
