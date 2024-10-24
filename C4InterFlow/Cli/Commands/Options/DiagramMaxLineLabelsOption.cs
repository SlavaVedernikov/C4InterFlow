using System;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Commands.Options;

public static class DiagramMaxLineLabelsOption
{
    public const int DefaultValue = 10;

    public static Option<int> Get()
    {
        const string description =
            "The numner of new-line delimited labels per a line on a Diagram.";

        var option = new Option<int>(new[] { "--max-line-labels", "-mll" }, description);
        option.SetDefaultValue(DefaultValue);

        return option;
    }

    
}
