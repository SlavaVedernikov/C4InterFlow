using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class OutputSubDirectoryOption
{
    public static Option<string> Get()
    {
        const string description =
            "The sub-directory where the Diagram(s) should be saved.";

        var option = new Option<string>(new[] { "--output-sub-dir", "-osd" }, description);

        option.SetDefaultValue(null);

        return option;
    }
}
