using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class InterfacesOption
{
    public static Option<string[]> Get()
    {
        const string description =
            "The aliases of the interfaces to be used as input for a Command";

        var option = new Option<string[]>(new[] { "--interfaces", "-i" }, description)
        {
            AllowMultipleArgumentsPerToken = true
        };
        option.SetDefaultValue(null);

        return option;
    }
}
