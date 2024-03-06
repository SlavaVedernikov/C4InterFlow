using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Options;

public static class TemplatesDirectoryOption
{
    public static Option<string> Get()
    {
        const string description =
           $"The directory where documentation templates for structures can be found";

        var option = new Option<string>(new[] { "--templates-dir", "-td" }, description);

        return option;
    }
}
