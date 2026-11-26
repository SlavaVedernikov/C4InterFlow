using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using C4InterFlow.Cli.Commands.Options;
using C4InterFlow.Cli.Configuration;
using Serilog;

namespace C4InterFlow.Cli.Commands;

public class ExportSchemaCommand : Command
{
    public const string COMMAND_NAME = "export-schema";

    public ExportSchemaCommand() : base(COMMAND_NAME,
        $"Exports '{CommandsSchemaBuilder.FILE_NAME}', the JSON schema of the commands that can be used in '{ConfigLoader.FILE_NAME}'. The schema is generated from the commands themselves.")
    {
        var outputDirectoryOption = OutputDirectoryOption.Get();

        AddOption(outputDirectoryOption);

        this.SetHandler((InvocationContext context) =>
        {
            Environment.ExitCode = Execute(context.Parser, context.ParseResult.GetValueForOption(outputDirectoryOption)!);
        });
    }

    private static int Execute(System.CommandLine.Parsing.Parser parser, string outputDirectory)
    {
        try
        {
            Log.Information("{Name} command is executing", COMMAND_NAME);

            var path = Path.Combine(Path.GetFullPath(outputDirectory), CommandsSchemaBuilder.FILE_NAME);
            var schema = CommandsSchemaBuilder.BuildText(parser);

            // Do not touch the file if nothing changed.
            if (File.Exists(path) && File.ReadAllText(path) == schema)
            {
                Log.Information("'{Path}' is up to date", path);
                return 0;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, schema, new UTF8Encoding(false));

            Log.Information("'{Path}' was updated", path);
            return 0;
        }
        catch (Exception e)
        {
            Log.Error(e, "{Name} command execution failed: {Error}", COMMAND_NAME, e.Message);

            return 1;
        }
    }
}
