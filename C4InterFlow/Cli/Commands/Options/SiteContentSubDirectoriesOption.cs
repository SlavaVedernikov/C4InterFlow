using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Options;

public static class SiteContentSubDirectoriesOption
{
    public static Option<string[]> Get()
    {
        const string description =
           $"The sub-directories where site content can be found.";

        var option = new Option<string[]>(new[] { "--site-content-sub-dirs", "-scsds" }, description)
        {
            AllowMultipleArgumentsPerToken = true
        };
        option.IsRequired = true;

        return option;
    }
}
