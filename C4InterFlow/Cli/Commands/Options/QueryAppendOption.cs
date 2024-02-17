using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class QueryAppendOption
{
    public static Option<bool> Get()
    {
        const string description = $"Append Query results to an output file. Default false.";

        var option = new Option<bool>(new[] { "--append", "-a" }, description);
        option.SetDefaultValue(false);

        return option;        
    }
}
