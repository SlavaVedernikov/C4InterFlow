using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class EnvironmentVariablesOption
{
    public static Option<string[]> Get()
    {
        const string description =
            "The expressions to be used to set environment variables before external batch file is executed e.g. SOME_VARIABLE=some-value.";

        var option = new Option<string[]>(new[] { "--environment-variables", "-ev" }, description)
        {
            AllowMultipleArgumentsPerToken = true
        };
        option.SetDefaultValue(null);

        return option;
    }
}
