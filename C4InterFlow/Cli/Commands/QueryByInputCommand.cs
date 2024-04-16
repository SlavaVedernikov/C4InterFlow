using System.CommandLine;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;
using C4InterFlow.Cli.Commands.Options;
using System.Reflection;
using C4InterFlow.Automation;

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
            Console.WriteLine($"'{COMMAND_NAME}' command is executing...");

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
            Console.WriteLine($"'{COMMAND_NAME}' command completed. See query results below.");
            Console.Write($"{string.Join(Environment.NewLine, result.Distinct().ToArray())}");
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"'{COMMAND_NAME}' command failed with exception '{e.Message}'");
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
