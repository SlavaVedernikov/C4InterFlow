using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class SoftwareSystemSolutionPathOption
{
    public static Option<string> Get()
    {
        const string description =
            "The path to a Solution for a Software System.";

        var option = new Option<string>(new[] { "--system-solution-path", "-ssp" }, description);

        return option;
    }
}
