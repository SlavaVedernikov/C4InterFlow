using System.CommandLine;
using C4InterFlow.Structures;
using C4InterFlow.Cli.Commands.Options;
using C4InterFlow.Automation;
using System.Reflection;
using C4InterFlow.Cli.Commands.Binders;
using C4InterFlow.Commons.Extensions;
using Serilog;
using Serilog.Events;

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
        var loggingLevelOption = LoggingLevelOptions.Get();
        var loggingOutputOptions = LoggingOutputOptions.Get();
        
        AddOption(architectureRootNamespaceOption);
        AddOption(architectureOutputPathOption);
        AddOption(architectureAsCodeWriterStrategyTypeOption);
        AddOption(paramsOption);
        AddOption(loggingLevelOption);
        AddOption(loggingOutputOptions);

        this.SetHandler(async (architectureRootNamespace, architectureOutputPath, architectureAsCodeWriterStrategyType, paramsOption, loggingOption) =>
            {
                await Execute(architectureRootNamespace, architectureOutputPath, architectureAsCodeWriterStrategyType, paramsOption, loggingOption);
            },
            architectureRootNamespaceOption, architectureOutputPathOption, architectureAsCodeWriterStrategyTypeOption, new ParametersBinder(paramsOption), new LoggingOptionsBinder(loggingOutputOptions, loggingLevelOption));
    }

    private static async Task<int> Execute(string architectureRootNamespace, string architectureOutputPath, string architectureAsCodeWriterStrategyType, Dictionary<string, string> architectureAsCodeParams, LoggingOptions loggingOptions)
    {
        try
        {
            Log.Logger = new LoggerConfiguration().CreateLogger(loggingOptions);

            Log.Information("{Name} command is executing", COMMAND_NAME);

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
            Log.Error(e, "{Name} command execution failed: {Error}", COMMAND_NAME, e.Message);

            return 1;
        }
    }
    
}
