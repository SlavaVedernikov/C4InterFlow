using System.CommandLine;

namespace C4InterFlow.Cli.Root;

public interface IRootCommandContext
{
    RootCommand? GetRootCommand();
    IRootCommandContext Add<TCommand>() where TCommand: Command;
    
    Task<int> InvokeAsync(string[] args);
}
