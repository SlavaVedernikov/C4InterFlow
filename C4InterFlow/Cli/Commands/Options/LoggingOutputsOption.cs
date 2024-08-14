using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Options;

public static class LoggingOutputsOption
{
    public const string CONSOLE = "console";
    public const string FILE = "file";

    private static Option<string[]>? _instance;

    public static string[] DefaultOutputs => new[] { CONSOLE };

    public static Option<string[]> Get()
    {
        if (_instance is not null)
            return _instance;

        const string description = "Specify logging output destinations.";

        _instance = new Option<string[]>(new[] { "--log-out", "-lo" }, description)
        {
            AllowMultipleArgumentsPerToken = true,
            IsRequired = false,
        };

        _instance.FromAmong(GetAllSupported());
        _instance.SetDefaultValue(DefaultOutputs);

        return _instance;
    }

    private static string[] GetAllSupported()
    {
        return new[]
        {
            CONSOLE,
            FILE
        };
    }
}