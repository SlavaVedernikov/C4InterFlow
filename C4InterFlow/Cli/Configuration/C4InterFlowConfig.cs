namespace C4InterFlow.Cli.Configuration;

/// <summary>
/// The content of the c4interflow.config.yaml file.
/// </summary>
public sealed record C4InterFlowConfig
{
    /// <summary>
    /// The commands to execute, in order.
    /// </summary>
    public IReadOnlyList<CommandConfig> Commands { get; init; } = Array.Empty<CommandConfig>();
}
