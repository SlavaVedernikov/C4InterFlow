using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class QueryIsRecursiveOption
{
    public static Option<bool> Get()
    {
        const string description = $"Is Query recursive. Default false.";

        var option = new Option<bool>(new[] { "--recursive", "-r" }, description);
        option.SetDefaultValue(false);

        return option;        
    }
}
