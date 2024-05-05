using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Options;

public static class SiteNoSitemapSubDirectoriesOption
{
    public static Option<string[]> Get()
    {
        const string description =
           $"The sub-directories that shoud be excluded from a sitemap.";

        var option = new Option<string[]>(new[] { "--site-no-sitemap-sub-dirs", "-snssds" }, description)
        {
            AllowMultipleArgumentsPerToken = true
        };
        option.SetDefaultValue(null);

        return option;
    }
}
