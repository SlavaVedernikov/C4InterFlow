using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class ArchitectureAsCodeInputPathOption
{
    public static Option<string> Get()
    {
        const string description =
            "The path to the Architecture As Code input.";

        var option = new Option<string>(new[] { "--aac-input-path", "-aac-inp" }, description);

        return option;
    }
}
