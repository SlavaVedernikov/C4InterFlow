using C4InterFlow.Structures;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class DiagramTypesOption
{
    public const string C4 = "c4";
    public const string C4_STATIC = "c4-static";
    public const string C4_SEQUENCE = "c4-sequence";
    public const string SEQUENCE = "sequence";
    public const string ENTITY = "entity";
    public const string ALL_DIADRAM_TYPES = "*";
    public static Option<string[]> Get()
    {
        const string description = $"Types of Diagram(s) to draw.";

        var option = new Option<string[]>(new[] { "--types", "-t" }, description)
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true
        };
        option.FromAmong(C4, C4_STATIC, C4_SEQUENCE, SEQUENCE, ENTITY, ALL_DIADRAM_TYPES);

        option.SetDefaultValue(new[] { ALL_DIADRAM_TYPES });

        return option;        
    }

    public static string[] GetAllDiagramTypes()
    {
        return new[] { C4, C4_STATIC, C4_SEQUENCE, SEQUENCE, ENTITY };
    }
}
