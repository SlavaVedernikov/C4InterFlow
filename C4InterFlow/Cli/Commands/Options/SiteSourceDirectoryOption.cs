using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Options;

public static class SiteSourceDirectoryOption
{
    public static Option<string> Get()
    {
        const string description =
           $"The directory where site source can be found";

        var option = new Option<string>(new[] { "--site-source-dir", "-ssd" }, description)
        {
            IsRequired = true
        };

        return option;
    }
}
