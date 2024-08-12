using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Options;

public static class LoggingOutputOptions
{
    public const string CONSOLE = "console";
    public const string FILE = "file";

    public static Option<string> Get()
    {
        const string description = "Specify logging output destination.";

        var option = new Option<string>(new[] { "--log-out", "-lo" }, description);

        option.FromAmong(CONSOLE, FILE);
        option.SetDefaultValue(CONSOLE);

        return option;
    }
}