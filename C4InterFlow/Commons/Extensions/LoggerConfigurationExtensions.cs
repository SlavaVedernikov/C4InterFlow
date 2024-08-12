using C4InterFlow.Cli.Commands.Binders;
using Serilog;
using Serilog.Core;

namespace C4InterFlow.Commons.Extensions;

public static class LoggerConfigurationExtensions
{
    public static Logger CreateLogger(this LoggerConfiguration configuration, LoggingOptions loggingOptions)
    {
        switch (loggingOptions.LoggingOutput)
        {
            case LoggingOutput.File:
                configuration.WriteTo.File("logs.txt", shared: true);
                break;
            case LoggingOutput.Console:
            default:
                configuration.WriteTo.Console();
                break;
        }

        configuration.MinimumLevel.Is(loggingOptions.LogEventLevel);

        return configuration.CreateLogger();
    }
}