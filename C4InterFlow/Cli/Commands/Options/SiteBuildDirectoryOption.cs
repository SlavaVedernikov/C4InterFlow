using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Options;

public static class SiteBuildDirectoryOption
{
    public static Option<string> Get()
    {
        const string description =
           $"The directory where built site can be found";

        var option = new Option<string>(new[] { "--site-build-dir", "-sbd" }, description);

        return option;
    }
}
