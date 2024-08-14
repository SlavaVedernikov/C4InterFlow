using C4InterFlow.Cli.Commands.Binders;
using C4InterFlow.Cli.Commands.Options;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace C4InterFlow.Commons.Extensions;

public static class LoggerConfigurationExtensions
{
    public static Logger CreateLogger(this LoggerConfiguration configuration, LoggingOptions loggingOptions)
    {
        foreach (var loggingOutput in loggingOptions.LoggingOutputs)
            switch (loggingOutput)
            {
                case LoggingOutputsOption.CONSOLE:
                    configuration.WriteTo.Console();
                    break;
                case LoggingOutputsOption.FILE:
                    configuration.WriteTo.File("logs.txt", shared: true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        var logEventLevel = loggingOptions.LogEventLevel switch
        {
            LoggingLevelOption.DEBUG => LogEventLevel.Debug,
            LoggingLevelOption.INFO => LogEventLevel.Information,
            LoggingLevelOption.WARNING => LogEventLevel.Warning,
            LoggingLevelOption.ERROR => LogEventLevel.Error,
        };
        configuration.MinimumLevel.Is(logEventLevel);
        return configuration.CreateLogger();
    }
}