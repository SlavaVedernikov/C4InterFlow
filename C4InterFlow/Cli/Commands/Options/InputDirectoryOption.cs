using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class InputDirectoryOption
{
    public static Option<string> Get()
    {
        const string description =
            "The input directory for the current command.";

        var option = new Option<string>(new[] { "--input-dir", "-id" }, description);

        option.SetDefaultValue(Directory.GetCurrentDirectory());

        return option;
    }
}
