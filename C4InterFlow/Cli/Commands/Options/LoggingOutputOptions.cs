using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Options;

public static class LoggingOutputOptions
{
    private static Option<IEnumerable<LoggingOutput>>? _isntance; 
    public static Option<IEnumerable<LoggingOutput>> Get()
    {
        if (_isntance is not null)
            return _isntance;
        
        const string description = "Specify logging output destinations.";

        _isntance = new Option<IEnumerable<LoggingOutput>>(new[] { "--log-out", "-lo" }, description)
        {
            AllowMultipleArgumentsPerToken = true,
            IsRequired = false,
        };
        _isntance.FromAmong(GetAllSupported());
        _isntance.SetDefaultValue(LoggingOutput.Console);

        return _isntance;
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