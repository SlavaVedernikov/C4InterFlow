using System.CommandLine;
using C4InterFlow.Elements;
using C4InterFlow.Cli.Commands.Options;
using C4InterFlow.Automation;
using System.Reflection;
using C4InterFlow.Cli.Commands.Binders;

namespace C4InterFlow.Cli.Commands;

public class ExecuteArchitectureAsCodeStrategyCommand : Command
{
    private const string COMMAND_NAME = "execute-aac-strategy";
    public ExecuteArchitectureAsCodeStrategyCommand() : base(COMMAND_NAME,
        "Executes Architecture As Code automation Strategy")
    {
        var architectureRootNamespaceOption = ArchitectureRootNamespaceOption.Get();
        var architectureOutputPathOption = ArchitectureAsCodeOutputPathOption.Get();
        var architectureAsCodeWriterStrategyTypeOption = ArchitectureAsCodeWriterStrategyTypeOption.Get();
        var architectureAsCodeParamsOption = ArchitectureAsCodeParamsOption.Get();

        AddOption(architectureRootNamespaceOption);
        AddOption(architectureOutputPathOption);
        AddOption(architectureAsCodeWriterStrategyTypeOption);
        AddOption(architectureAsCodeParamsOption);

        this.SetHandler(async (architectureRootNamespace, architectureOutputPath, architectureAsCodeWriterStrategyType, architectureAsCodeParamsOption) =>
            {
                await Execute(architectureRootNamespace, architectureOutputPath, architectureAsCodeWriterStrategyType, architectureAsCodeParamsOption);
            },
            architectureRootNamespaceOption, architectureOutputPathOption, architectureAsCodeWriterStrategyTypeOption, new ParametersBinder(architectureAsCodeParamsOption));
    }

    private static async Task<int> Execute(string architectureRootNamespace, string architectureOutputPath, string architectureAsCodeWriterStrategyType, Dictionary<string, string> architectureAsCodeParams)
    {
        try
        {
            Type strategyType = Type.GetType(architectureAsCodeWriterStrategyType);
            object strategyTypeInstance = Activator.CreateInstance(strategyType);
            var strategyInstance = strategyTypeInstance as ArchitectureAsCodeWriterStrategy;

            if (strategyInstance == null)
            {
                throw new ArgumentException($"'{architectureAsCodeWriterStrategyType}' is not a valid Architecture As Code Strategy type.");
            }

            var context = new ArchitectureAsCodeWriterContext(strategyInstance, architectureRootNamespace, architectureOutputPath, architectureAsCodeParams);
            context.ExecuteStrategy();
            
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Architecture As Code automation Strategy execution failed: '{e.Message}'");
            return 1;
        }
    }

    private static IEnumerable<string> GetUsedBy(IEnumerable<Type> interfaceTypes, string interfaceAlias, bool isRecursive, List<string> usedByResult)
    {
        var result = new List<string>();

        foreach (var interfaceType in interfaceTypes)
        {
            var interfaceInstance = interfaceType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null, null) as Interface;

            if (interfaceInstance?.Flow.GetUsesInterfaces().Select(x => x.Alias).Contains(interfaceAlias) == true)
            {
                if (isRecursive)
                {
                    var tempResult = GetUsedBy(interfaceTypes, interfaceInstance.Alias, isRecursive, usedByResult);
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

        usedByResult.AddRange(result);
        return result;
    }
}
