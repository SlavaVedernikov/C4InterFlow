using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class InterfacesInputFileOption
{
    public static Option<string> Get()
    {
        const string description =
            "The file where the aliases of the interfases for which to draw the Diagram(s) can be loaded from.";

        var option = new Option<string>(new[] { "--interfaces-input-file", "-iif" }, description);
        option.SetDefaultValue(null);

        return option;
    }
}
