using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class ShowBoundariesOption
{
    public static Option<bool?> Get()
    {
        const string description = $"Show boundaries. NOTE: This option is used for '{DiagramLevelsOfDetailsOption.COMPONENT}' level of details only.";

        var option = new Option<bool?>(new[] { "--show-boundaries", "-sb" }, description);
        option.SetDefaultValue(false);

        return option;        
    }
}
