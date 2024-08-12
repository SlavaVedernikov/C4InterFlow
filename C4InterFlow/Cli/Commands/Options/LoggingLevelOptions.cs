using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Options;

public static class LoggingLevelOptions
{
    public const string DEBUG = "debug";
    public const string INFO = "info";
    public const string WARNING = "warning";
    public const string ERROR = "error";

    public static Option<string> Get()
    {
        const string description = "Specifies the meaning and relative importance of a log event.";

        var option = new Option<string>(new[] { "--log-level", "-ll" }, description);

        option.FromAmong(DEBUG, INFO, WARNING, ERROR);
        option.SetDefaultValue(INFO);

        return option;
    }
}