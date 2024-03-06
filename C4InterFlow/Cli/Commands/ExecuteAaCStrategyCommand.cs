using System.CommandLine;
using C4InterFlow.Structures;
using C4InterFlow.Cli.Commands.Options;
using C4InterFlow.Automation;
using System.Reflection;
using C4InterFlow.Cli.Commands.Binders;

namespace C4InterFlow.Cli.Commands;

public class ExecuteAaCStrategyCommand: Command
{
    private const string COMMAND_NAME = "execute-aac-strategy";
    public ExecuteAaCStrategyCommand() : base(COMMAND_NAME,
        "Executes Architecture As Code generation Strategy")
    {
        var architectureRootNamespaceOption = ArchitectureRootNamespaceOption.Get();
        var architectureOutputPathOption = AaCOutputPathOption.Get();
        var architectureAsCodeWriterStrategyTypeOption = AaCWriterStrategyTypeOption.Get();
        var paramsOption = ParamsOption.Get();

        AddOption(architectureRootNamespaceOption);
        AddOption(architectureOutputPathOption);
        AddOption(architectureAsCodeWriterStrategyTypeOption);
        AddOption(paramsOption);

        this.SetHandler(async (architectureRootNamespace, architectureOutputPath, architectureAsCodeWriterStrategyType, paramsOption) =>
            {
                await Execute(architectureRootNamespace, architectureOutputPath, architectureAsCodeWriterStrategyType, paramsOption);
            },
            architectureRootNamespaceOption, architectureOutputPathOption, architectureAsCodeWriterStrategyTypeOption, new ParametersBinder(paramsOption));
    }

    private static async Task<int> Execute(string architectureRootNamespace, string architectureOutputPath, string architectureAsCodeWriterStrategyType, Dictionary<string, string> architectureAsCodeParams)
    {
        try
        {
            Console.WriteLine($"'{COMMAND_NAME}' command is executing...");

            Type strategyType = Type.GetType(architectureAsCodeWriterStrategyType);

            if (strategyType == null)
            {
                throw new ArgumentException($"Cannot find type '{architectureAsCodeWriterStrategyType}'.");
            }

            object strategyTypeInstance = Activator.CreateInstance(strategyType);
            var strategyInstance = strategyTypeInstance as AaCWriterStrategy;

            if (strategyInstance == null)
            {
                throw new ArgumentException($"Cannot load AaC Writer Strategy type'{architectureAsCodeWriterStrategyType}'.");
            }

            var context = new AaCWriterContext(strategyInstance, architectureRootNamespace, architectureOutputPath, architectureAsCodeParams);
            
            context.ExecuteStrategy();
            
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Architecture As Code automation Strategy execution failed: '{e.Message}'");
            return 1;
        }
    }
    
}
