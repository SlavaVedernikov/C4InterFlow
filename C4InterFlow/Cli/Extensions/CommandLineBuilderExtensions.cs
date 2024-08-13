using System.CommandLine.Builder;
using C4InterFlow.Cli.Commands.Binders;
using C4InterFlow.Cli.Commands.Options;
using C4InterFlow.Commons.Extensions;
using Serilog;

namespace C4InterFlow.Cli.Extensions;

public static class CommandLineBuilderExtensions
{
    public static CommandLineBuilder UseLogging(this CommandLineBuilder builder)
    {
        return builder.AddMiddleware(async (context, next) =>
        {
            var loggingOutput = context.BindingContext.ParseResult.GetValueForOption(LoggingOutputOptions.Get());
            var loggingLevel = context.BindingContext.ParseResult.GetValueForOption(LoggingLevelOptions.Get());

            Console.WriteLine($"Logging level: {loggingLevel}");
            Console.WriteLine($"Logging outputs: {string.Join(',', loggingOutput ?? Array.Empty<LoggingOutput>())}");

            var loggingOptions = new LoggingOptions(loggingOutput, loggingLevel);
            Log.Logger = new LoggerConfiguration().CreateLogger(loggingOptions);

            await next(context);
        });
    }
}