using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class SoftwareSystemNameOption
{
    public static Option<string> Get()
    {
        const string description =
            "The name of a Software System.";

        var option = new Option<string>(new[] { "--system-name", "-sn" }, description);

        return option;
    }
}
