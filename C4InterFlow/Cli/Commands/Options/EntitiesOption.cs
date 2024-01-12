using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class EntitiesOption
{
    public static Option<string[]> Get()
    {
        const string description =
            "The aliases of the entities for which to draw the Diagram(s).";

        var option = new Option<string[]>(new[] { "--entities", "-e" }, description)
        {
            AllowMultipleArgumentsPerToken = true
        };
        option.SetDefaultValue(null);

        return option;
    }
}
