using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class ViewsOption
{
    public static Option<string[]> Get()
    {
        const string description =
            "The aliases of the views to be used as input for a Command";

        var option = new Option<string[]>(new[] { "--views", "-v" }, description)
        {
            AllowMultipleArgumentsPerToken = true
        };
        option.IsRequired = true;

        return option;
    }
}
