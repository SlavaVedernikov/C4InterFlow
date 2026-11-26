using System.CommandLine.Builder;
using C4InterFlow.Cli.Commands.Binders;
using C4InterFlow.Cli.Commands.Options;
using C4InterFlow.Commons.Extensions;
using Serilog;

namespace C4InterFlow.Cli.Extensions;

public static class CommandLineBuilderExtensions
{
    // A command can invoke other commands (e.g. execute-commands). Logging is configured once,
    // from the options of the outermost invocation, and is not reset by the nested ones.
    private static bool _isLoggingConfigured;

    public static CommandLineBuilder UseLogging(this CommandLineBuilder builder)
    {
        return builder.AddMiddleware(async (context, next) =>
        {
            if (!_isLoggingConfigured)
            {
                var loggingOutput = context.BindingContext.ParseResult.GetValueForOption(LoggingOutputsOption.Get());
                var loggingLevel = context.BindingContext.ParseResult.GetValueForOption(LoggingLevelOption.Get());

                var loggingOptions = new LoggingOptions(loggingOutput, loggingLevel);
                Log.Logger = new LoggerConfiguration().CreateLogger(loggingOptions);
                _isLoggingConfigured = true;
            }

            await next(context);
        });
    }
}