using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class ArchitectureAsCodeWriterStrategyTypeOption
{
    public static Option<string> Get()
    {
        const string description =
            "The full Type name of the Architecture As Code Writer Strategy to execute.";

        var option = new Option<string>(new[] { "--aac-writer-strategy", "-aac-ws" }, description)
        {
            IsRequired = true
        };

        return option;
    }
}
