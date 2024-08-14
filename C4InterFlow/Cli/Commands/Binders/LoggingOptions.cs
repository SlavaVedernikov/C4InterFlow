using System.CommandLine;
using System.CommandLine.Binding;
using C4InterFlow.Cli.Commands.Options;
using Serilog.Events;

namespace C4InterFlow.Cli.Commands.Binders;

public class LoggingOptions
{
    public LoggingOptions(string[]? loggingOutputs, string? logLevel)
    {
        LoggingOutputs = loggingOutputs?.Concat(LoggingOutputsOption.DefaultOutputs).Distinct().ToArray() ?? LoggingOutputsOption.DefaultOutputs;
        LogEventLevel = logLevel ?? LoggingLevelOption.DefaultEventLevel;
    }

    public string LogEventLevel { get; private set; }
    public string[] LoggingOutputs { get; private set; }
}

public class LoggingOptionsBinder : BinderBase<LoggingOptions>
{
    private readonly Option<string[]> _loggingOutput;
    private readonly Option<string> _loglevel;

    public LoggingOptionsBinder(Option<string[]> loggingOutput, Option<string> loglevel)
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