using System.CommandLine;
using System.CommandLine.Binding;
using C4InterFlow.Cli.Commands.Options;
using Serilog.Events;

namespace C4InterFlow.Cli.Commands.Binders;

public class LoggingOptions
{
    private readonly LoggingOutput[] _defaultLoggingOutputs = { LoggingOutput.Console };

    public LoggingOptions(IEnumerable<LoggingOutput>? loggingOutputs, LogEventLevel? logLevel)
    {
        LoggingOutputs = loggingOutputs?.Concat(_defaultLoggingOutputs).Distinct().ToArray() ?? _defaultLoggingOutputs;
        LogEventLevel = logLevel ?? LogEventLevel.Information;
    }

    public LogEventLevel LogEventLevel { get; private set; }
    public LoggingOutput[] LoggingOutputs { get; private set; }
}

public class LoggingOptionsBinder : BinderBase<LoggingOptions>
{
    private readonly Option<LoggingOutput[]> _loggingOutput;
    private readonly Option<LogEventLevel> _loglevel;

    public LoggingOptionsBinder(Option<LoggingOutput[]> loggingOutput, Option<LogEventLevel> loglevel)
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