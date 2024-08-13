using C4InterFlow.Cli.Commands.Binders;
using C4InterFlow.Cli.Commands.Options;
using Serilog;
using Serilog.Core;

namespace C4InterFlow.Commons.Extensions;

public static class LoggerConfigurationExtensions
{
    public static Logger CreateLogger(this LoggerConfiguration configuration, LoggingOptions loggingOptions)
    {
        foreach (var loggingOutput in loggingOptions.LoggingOutputs)
            switch (loggingOutput)
            {
                case LoggingOutput.Console:
                    configuration.WriteTo.Console();
                    break;
                case LoggingOutput.File:
                    configuration.WriteTo.File("logs.txt", shared: true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        configuration.MinimumLevel.Is(loggingOptions.LogEventLevel);
        return configuration.CreateLogger();
    }
}