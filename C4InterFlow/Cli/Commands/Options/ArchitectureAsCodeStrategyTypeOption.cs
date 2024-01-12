using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class ArchitectureAsCodeStrategyTypeOption
{
    public static Option<string> Get()
    {
        const string description =
            "The full Type name of the Architecture As Code Strategy to execute.";

        var option = new Option<string>(new[] { "--aac-strategy", "-aac-s" }, description);

        return option;
    }
}
