using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class ClearOutputDirectoryOption
{
    public static Option<bool> Get()
    {
        const string description = $"Clear output directory before drawing diagrams.";

        var option = new Option<bool>(new[] { "--clear-output-dir", "-c" }, description);
        option.SetDefaultValue(false);

        return option;        
    }
}
