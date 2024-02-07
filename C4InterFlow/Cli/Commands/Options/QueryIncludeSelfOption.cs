using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class QueryIncludeSelfOption
{
    public static Option<bool> Get()
    {
        const string description = $"Is structure queried by included in the query result. Default false.";

        var option = new Option<bool>(new[] { "--include-self", "-incls" }, description);
        option.SetDefaultValue(false);

        return option;        
    }
}
