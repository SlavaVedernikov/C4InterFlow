using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class AaCOutputPathOption
{
    public static Option<string> Get()
    {
        const string description =
            "The path to the Architecture As Code output.";

        var option = new Option<string>(new[] { "--aac-output-path", "-aac-outp" }, description)
        {
            IsRequired = true
        };

        return option;
    }
}
