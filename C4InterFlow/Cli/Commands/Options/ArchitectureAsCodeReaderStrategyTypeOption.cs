using C4InterFlow.Structures;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class ArchitectureAsCodeReaderStrategyTypeOption
{
    public static Option<string> Get()
    {
        const string description = $"The full Type name of the Architecture As Code Reader Strategy to use.";

        var option = new Option<string>(new[] { "--aac-reader-strategy", "-aac-rs" }, description)
        {
            IsRequired = true
        };

        return option;        
    }
}
