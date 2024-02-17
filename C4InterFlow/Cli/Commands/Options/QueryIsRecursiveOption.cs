using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class QueryIsRecursiveOption
{
    public static Option<bool> Get()
    {
        const string description = $"Indicates if the Query should be executed recursively. Default false.";

        var option = new Option<bool>(new[] { "--recursive", "-r" }, description);
        option.SetDefaultValue(false);

        return option;        
    }
}
