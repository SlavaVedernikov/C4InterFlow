using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class AaCParamsOption
{
    public static Option<string[]> Get()
    {
        const string description =
            "The key/value pairs of additional AaC Strategy parameters.";

        var option = new Option<string[]>(new[] { "--aac-params", "-aac-p" }, description);

        return option;
    }
}
