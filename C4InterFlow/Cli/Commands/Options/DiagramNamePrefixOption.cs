using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class DiagramNamePrefixOption
{
    public static Option<string> Get()
    {
        const string description =
            "The name prefix for a Diagram.";

        var option = new Option<string>(new[] { "--name-prefix", "-np" }, description);
        option.SetDefaultValue(null);

        return option;
    }
}
