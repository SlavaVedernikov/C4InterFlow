using System.CommandLine;
using System.CommandLine.Binding;
using C4InterFlow.Cli.Commands.Options;
using Serilog.Events;

namespace C4InterFlow.Cli.Commands.Binders;

public class LoggingOptions
{
    public LoggingOptions(string? loggingOutput, string? logLevel)
    {
        LoggingOutput = Enum.TryParse(loggingOutput, true, out LoggingOutput output) ? output : LoggingOutput.Console;
        
        if (logLevel == LoggingLevelOptions.INFO)
            LogEventLevel = LogEventLevel.Information;
        else
            LogEventLevel = Enum.TryParse(logLevel, true, out LogEventLevel level) ? level : LogEventLevel.Information;
    }

    public LogEventLevel LogEventLevel { get; private set; }
    public LoggingOutput LoggingOutput { get; private set; }
}

public class LoggingOptionsBinder : BinderBase<LoggingOptions>
{
    private readonly Option<string> _loggingOutput;
    private readonly Option<string> _loglevel;

    public LoggingOptionsBinder(Option<string> loggingOutput, Option<string> loglevel)
    {
        _loggingOutput = loggingOutput;
        _loglevel = loglevel;
    }

    protected override LoggingOptions GetBoundValue(BindingContext bindingContext)
    {
        return new LoggingOptions(bindingContext.ParseResult.GetValueForOption(_loggingOutput),
            bindingContext.ParseResult.GetValueForOption(_loglevel));
    }
}

public enum LoggingOutput
{
    Console = 0,
    File = 1
}