using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class NamespacesOption
{
    public static Option<string[]> Get()
    {
        const string description =
            "The aliases of Namespaces for which to draw the Diagram(s).";

        var option = new Option<string[]>(new[] { "--namespaces", "-ns" }, description)
        {
            AllowMultipleArgumentsPerToken = true
        };
        option.SetDefaultValue(null);

        return option;
    }
}
