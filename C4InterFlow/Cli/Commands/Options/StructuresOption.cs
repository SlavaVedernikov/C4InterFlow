using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class StructuresOption
{
    public static Option<string> Get()
    {
        const string description =
            "The alias/query of the structure(s) to be used as input for a Command";

        var option = new Option<string>(new[] { "--structures", "-st" }, description)
        {
            
        };
        option.SetDefaultValue(null);

        return option;
    }
}
