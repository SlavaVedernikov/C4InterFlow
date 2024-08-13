using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Options;

public static class LoggingOutputOptions
{
    private static Option<IEnumerable<LoggingOutput>>? _instance;

    public static LoggingOutput[] DefaultOutputs => new[] { LoggingOutput.Console };

    public static Option<IEnumerable<LoggingOutput>> Get()
    {
        if (_instance is not null)
            return _instance;

        const string description = "Specify logging output destinations.";

        _instance = new Option<IEnumerable<LoggingOutput>>(new[] { "--log-out", "-lo" }, description)
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
        return Enum.GetNames<LoggingOutput>().Select(x => x.ToLowerInvariant()).ToArray();
    }
}

public enum LoggingOutput
{
    Console = 0,
    File = 1
}