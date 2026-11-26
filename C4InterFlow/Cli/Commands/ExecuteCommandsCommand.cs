using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using C4InterFlow.Automation;
using C4InterFlow.Cli.Configuration;
using Serilog;

namespace C4InterFlow.Cli.Commands;

public class ExecuteCommandsCommand : Command
{
    public const string COMMAND_NAME = "execute-commands";

    public ExecuteCommandsCommand() : base(COMMAND_NAME,
        $"Executes the commands defined in '{ConfigLoader.FILE_NAME}' found in the current directory, in order. Stops at the first failure.")
    {
        this.SetHandler(async (InvocationContext context) =>
        {
            Environment.ExitCode = await Execute(context.Parser);
        });
    }

    private static async Task<int> Execute(Parser parser)
    {
        try
        {
            Log.Information("{Name} command is executing", COMMAND_NAME);

            var currentDirectory = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(currentDirectory, ConfigLoader.FILE_NAME);

            if (!File.Exists(configPath))
            {
                Log.Error("'{ConfigFile}' was not found in '{Directory}'.", ConfigLoader.FILE_NAME, currentDirectory);
                return 1;
            }

            if (!ConfigLoader.TryLoad(configPath, parser, out var config, out var errors))
            {
                foreach (var error in errors)
                {
                    Log.Error("{ConfigFile}: {Error}", ConfigLoader.FILE_NAME, error);
                }

                Log.Error("'{ConfigFile}' is not valid. Please resolve the errors above and retry.", ConfigLoader.FILE_NAME);
                return 1;
            }

            var commands = config!.Commands;

            // Fail before running anything if any 'clearOutputDir' target is unsafe.
            for (var i = 0; i < commands.Count; i++)
            {
                if (commands[i].ClearOutputDir && !TryGetCleanDirectory(commands[i], currentDirectory, out _, out var error))
                {
                    Log.Error("commands[{Index}] ({Command}): {Error}", i, commands[i].Command, error);
                    return 1;
                }
            }

            for (var i = 0; i < commands.Count; i++)
            {
                var command = commands[i];
                var position = $"[{i + 1}/{commands.Count}]";
                var arguments = command.ToArguments().ToArray();

                Log.Information("{Position} {Command}", position, command.Command);
                foreach (var line in Describe(command))
                {
                    Log.Information("{Line}", line);
                }
                Log.Information("  equivalent command line: {CommandLine}", FormatCommandLine(arguments));

                if (command.ClearOutputDir)
                {
                    TryGetCleanDirectory(command, currentDirectory, out var cleanDirectory, out _);
                    ClearDirectory(cleanDirectory);
                }

                // Every command sets up its own Architecture As Code reader.
                AaCReaderContext.Reset();
                Environment.ExitCode = 0;

                var exitCode = await parser.InvokeAsync(arguments);
                if (Environment.ExitCode != 0)
                {
                    exitCode = Environment.ExitCode;
                }

                if (exitCode != 0)
                {
                    Log.Error("{Position} {Command} failed. Stopping.", position, command.Command);
                    return exitCode;
                }
            }

            Log.Information("{Name} command completed", COMMAND_NAME);
            return 0;
        }
        catch (Exception e)
        {
            Log.Error(e, "{Name} command execution failed: {Error}", COMMAND_NAME, e.Message);

            return 1;
        }
    }

    private static IEnumerable<string> Describe(CommandConfig command)
    {
        if (command.ClearOutputDir)
        {
            yield return "  clearOutputDir: true (contents of the output directory will be deleted first)";
        }

        yield return "  options:";
        foreach (var option in command.Options)
        {
            if (option.IsList)
            {
                yield return $"    {option.Key}:";
                foreach (var value in option.Values)
                {
                    yield return $"      - {value}";
                }
            }
            else
            {
                yield return $"    {option.Key}: {option.Values[0]}";
            }
        }
    }

    private static string FormatCommandLine(IEnumerable<string> arguments)
    {
        return string.Join(" ", arguments.Select(x => x.Any(char.IsWhiteSpace) ? $"\"{x}\"" : x));
    }

    private static bool TryGetCleanDirectory(CommandConfig command, string rootDirectory, out string directory, out string error)
    {
        directory = string.Empty;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(command.OutputDir))
        {
            error = "'clearOutputDir' requires 'options.outputDir'.";
            return false;
        }

        var fullPath = Path.GetFullPath(command.OutputDir, rootDirectory);
        var relativePath = Path.GetRelativePath(rootDirectory, fullPath);

        if (relativePath == "." ||
            relativePath == ".." ||
            relativePath.StartsWith(".." + Path.DirectorySeparatorChar) ||
            relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar) ||
            Path.IsPathRooted(relativePath))
        {
            error = $"'clearOutputDir' can only clear a directory inside '{rootDirectory}', but 'outputDir' resolves to '{fullPath}'.";
            return false;
        }

        directory = fullPath;
        return true;
    }

    private static void ClearDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Log.Information("  '{Directory}' does not exist, nothing to clear.", directory);
            return;
        }

        var info = new DirectoryInfo(directory);
        foreach (var file in info.GetFiles())
        {
            file.Delete();
        }
        foreach (var subDirectory in info.GetDirectories())
        {
            subDirectory.Delete(true);
        }

        Log.Information("  Cleared '{Directory}'.", directory);
    }
}
