using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Options;

public static class DocumentationTemplateFileOption
{
    public static Option<string> Get()
    {
        const string description =
           $"The template file to be used to generate the documentation";

        var option = new Option<string>(new[] { "--template-file", "-tf" }, description);

        return option;
    }
}
