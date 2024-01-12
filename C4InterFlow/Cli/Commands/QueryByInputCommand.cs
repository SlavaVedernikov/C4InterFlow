using System.CommandLine;
using C4InterFlow.Elements;
using C4InterFlow.Elements.Interfaces;
using C4InterFlow.Cli.Commands.Options;
using System.Reflection;

namespace C4InterFlow.Cli.Commands;

public class QueryByInputCommand : Command
{
    private const string COMMAND_NAME = "query-by-input";
    public QueryByInputCommand() : base(COMMAND_NAME,
        "Queries interfaces by alias values in Interface 'Input' field.")
    {
        var entitiesOption = EntitiesOption.Get();

        AddOption(entitiesOption);

        this.SetHandler(async (entityAliases) =>
            {
                await Execute(entityAliases);
            },
            entitiesOption);
    }

    private static async Task<int> Execute(string[] entityAliases)
    {
        try
        {
            Console.WriteLine($"{COMMAND_NAME} command is executing...");

            entityAliases = Utils.ResolveWildcardStructures(entityAliases);
            var result = new List<string>();
            var interfaceTypes = Utils.GetAllTypesOfInterface<IInterfaceInstance>();
            
            foreach (var entityAlias in entityAliases)
            {
                result.AddRange(GetByInput(interfaceTypes, entityAlias));
            }
            Console.WriteLine($"{COMMAND_NAME} command completed. See query results below.");
            Console.Write($"{string.Join(Environment.NewLine, result.Distinct().ToArray())}");
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Querying interfaces failed with exception '{e.Message}'");
            return 1;
        }
    }

    private static IEnumerable<string> GetByInput(IEnumerable<Type> interfaceTypes, string entityAlias)
    {
        var result = new List<string>();

        foreach (var interfaceType in interfaceTypes)
        {
            var interfaceInstance = interfaceType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null, null) as Interface;

            if (interfaceInstance?.Input?.Equals(entityAlias) == true)
            {  
                result.Add(interfaceInstance.Alias);
            }
        }

        return result;
    }
}
