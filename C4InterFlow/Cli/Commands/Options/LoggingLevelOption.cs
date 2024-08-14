using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Options;

public static class LoggingLevelOption
{
    public const string DEBUG = "debug";
    public const string INFO = "info";
    public const string WARNING = "warning";
    public const string ERROR = "error";

    private static Option<string>? _instance;
    public static string DefaultEventLevel => INFO;

    public static Option<string> Get()
    {
        if (_instance is not null)
            return _instance;

        const string description = "Specifies the meaning and relative importance of a log event.";
        _instance = new Option<string>(new[] { "--log-level", "-ll" },
            description)
        {
            IsRequired = false
        };

        _instance.FromAmong(GetAllSupported());
        _instance.SetDefaultValue(DefaultEventLevel);

        return _instance;
    }

    private static string[] GetAllSupported()
    {
        return new[]
        {
            DEBUG,
            INFO,
            WARNING,
            ERROR
        };
    }
}