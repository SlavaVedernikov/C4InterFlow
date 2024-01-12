using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class ShowSequenceOption
{
    public static Option<bool> Get()
    {
        const string description = $"Show sequence. NOTE: This option is used for {DiagramTypesOption.C4} diagram type only.";

        var option = new Option<bool>(new[] { "--show-sequence", "-ss" }, description);
        option.SetDefaultValue(false);

        return option;        
    }
}
