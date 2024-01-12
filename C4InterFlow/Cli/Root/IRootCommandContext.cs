using System.CommandLine;

namespace C4InterFlow.Cli.Root;

public interface IRootCommandContext
{
    IRootCommandContext Add<TCommand>() where TCommand: Command;
    
    Task<int> InvokeAsync(string[] args);
}
