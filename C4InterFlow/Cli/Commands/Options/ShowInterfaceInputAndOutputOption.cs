using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class ShowInterfaceInputAndOutputOption
{
    public static Option<bool> Get()
    {
        const string description = $"Show Interface Input and Output. NOTE: This option is used for '{DiagramLevelsOfDetailsOption.COMPONENT}' level of details only.";

        var option = new Option<bool>(new[] { "--show-interface-input-output", "-siinout" }, description);
        option.SetDefaultValue(false);

        return option;        
    }
}
