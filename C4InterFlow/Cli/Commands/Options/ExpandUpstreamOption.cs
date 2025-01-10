using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class ExpandUpstreamOption
{
    public static Option<bool> Get()
    {
        const string description = $"Expand the diagram upstream from the scoped structure. NOTE: This option is used for '{DiagramTypesOption.C4}' and '{DiagramTypesOption.C4_STATIC}' diagram types only.";

        var option = new Option<bool>(new[] { "--expand-upstream", "-eu" }, description);
        option.SetDefaultValue(false);

        return option;        
    }
}
