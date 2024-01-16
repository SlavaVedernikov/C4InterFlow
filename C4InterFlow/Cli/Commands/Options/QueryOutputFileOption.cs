using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class QueryOutputFileOption
{
    public static Option<string> Get()
    {
        const string description =
            "The name of the file where the Query results should be saved.";

        var option = new Option<string>(new[] { "--query-output-file", "-qof" }, description);
        option.SetDefaultValue(null);

        return option;
    }
}
