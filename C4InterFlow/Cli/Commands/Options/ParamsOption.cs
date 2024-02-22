using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class ParamsOption
{
    public static Option<string[]> Get()
    {
        const string description =
            "The key/value pairs of additional command parameters.";

        var option = new Option<string[]>(new[] { "--params", "-ps" }, description);

        return option;
    }
}
