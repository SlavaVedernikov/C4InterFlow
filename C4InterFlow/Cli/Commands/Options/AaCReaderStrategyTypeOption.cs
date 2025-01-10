using C4InterFlow.Structures;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class AaCReaderStrategyTypeOption
{
    public static Option<string> Get()
    {
        const string description = $"The full Type name of the Architecture As Code Reader Strategy to use. Supports shortcuts for known strategies e.g. Yaml, Json and CSharp.";

        var option = new Option<string>(new[] { "--aac-reader-strategy", "-aac-rs" }, description)
        {
            IsRequired = true
        };

        return option;        
    }
}
