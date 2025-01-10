using C4InterFlow.Visualisation;
using C4InterFlow.Structures;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class DiagramLevelsOfDetailsOption
{
    public const string CONTEXT = "context";
    public const string CONTAINER = "container";
    public const string COMPONENT = "component";
    public const string ALL_LEVELS_OF_DETAILS = "*";
    public static Option<string[]> Get()
    {
        const string description = $"Levels of details of the Diagram(s).";

        var option = new Option<string[]>(new[] { "--levels-of-details", "-l" }, description)
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true
        };
        option.FromAmong(CONTEXT, CONTAINER, COMPONENT, ALL_LEVELS_OF_DETAILS);

        option.SetDefaultValue(new[] { ALL_LEVELS_OF_DETAILS });

        return option;
    }

    public static string[] GetAllValues()
    {
        return new[] { CONTEXT, CONTAINER, COMPONENT };
    }
}
