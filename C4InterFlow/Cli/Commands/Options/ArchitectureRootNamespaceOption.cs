using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class ArchitectureRootNamespaceOption
{
    public static Option<string> Get()
    {
        const string description =
            "Architecture As Code root Namespace.";

        var option = new Option<string>(new[] { "--aac-root-namespace", "-aac-rn" }, description)
        {
            IsRequired = true
        };

        return option;
    }
}
