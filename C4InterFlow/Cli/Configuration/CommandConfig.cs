namespace C4InterFlow.Cli.Configuration;

/// <summary>
/// A command to execute with its options.
/// </summary>
/// <param name="Command">The name of the command.</param>
/// <param name="ClearOutputDir">If true, the contents of the output directory are deleted before the command runs.</param>
/// <param name="Options">The options that were specified for the command, in the order they were specified.</param>
public sealed record CommandConfig(string Command, bool ClearOutputDir, IReadOnlyList<CommandOption> Options)
{
    private const string OUTPUT_DIR_CLI_OPTION = "--output-dir";

    /// <summary>
    /// The value of the 'outputDir' option, or null if it was not specified.
    /// </summary>
    public string? OutputDir => Options.FirstOrDefault(x => x.CliOption == OUTPUT_DIR_CLI_OPTION)?.Values is { Count: > 0 } values
        ? values[0]
        : null;

    /// <summary>
    /// The CLI arguments that execute the command with its options.
    /// </summary>
    public IEnumerable<string> ToArguments()
    {
        yield return Command;

        foreach (var option in Options)
        {
            yield return option.CliOption;

            foreach (var value in option.Values)
            {
                yield return value;
            }
        }
    }
}

/// <summary>
/// An option of a command.
/// </summary>
/// <param name="Key">The name of the option in the configuration file (e.g. aacInputPaths).</param>
/// <param name="CliOption">The CLI option (e.g. --aac-input-paths).</param>
/// <param name="Values">The values of the option. A single value for options that are not lists.</param>
/// <param name="IsList">True if the option was specified as a list.</param>
public sealed record CommandOption(string Key, string CliOption, IReadOnlyList<string> Values, bool IsList);
