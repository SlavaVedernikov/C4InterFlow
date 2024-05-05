using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Options;

public static class BatchFileOption
{
    public static Option<string> Get()
    {
        const string description =
           $"The path to .bat (batch) file to be executed by the command";

        var option = new Option<string>(new[] { "--batch-file", "-bf" }, description);

        return option;
    }
}
