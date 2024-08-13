using System.CommandLine;
using System.CommandLine.Parsing;
using Serilog.Events;

namespace C4InterFlow.Cli.Commands.Options;

public static class LoggingLevelOptions
{
    private static Option<LogEventLevel>? _instance;
    public static LogEventLevel DefaultEventLevel => LogEventLevel.Information;
    public static Option<LogEventLevel> Get()
    {
        if (_instance is not null)
            return _instance;

        const string description = "Specifies the meaning and relative importance of a log event.";
        _instance = new Option<LogEventLevel>(new[] { "--log-level", "-ll" },description)
        {
            IsRequired = false,
        };

        _instance.FromAmong(GetAllSupported());
        _instance.SetDefaultValue(DefaultEventLevel);

        return _instance;
    }

    private static string[] GetAllSupported()
    {
        return Enum.GetNames<LogEventLevel>().Select(x => x.ToLowerInvariant()).ToArray();
    }
}