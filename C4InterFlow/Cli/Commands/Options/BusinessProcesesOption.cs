using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class BusinessProcesesOption
{
    public static Option<string[]> Get()
    {
        const string description =
            "The aliases of Business Processes for which to draw the Diagram(s).";

        var option = new Option<string[]>(new[] { "--business-processes", "-bp" }, description)
        {
            AllowMultipleArgumentsPerToken = true
        };
        option.SetDefaultValue(null);

        return option;
    }
}
