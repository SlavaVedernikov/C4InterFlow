using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class StructuresOption
{
    public static Option<string[]> Get()
    {
        const string description =
            "The aliases of the structures to be used as input for a Command";

        var option = new Option<string[]>(new[] { "--structures", "-st" }, description)
        {
            AllowMultipleArgumentsPerToken = true
        };
        option.SetDefaultValue(null);

        return option;
    }
}
