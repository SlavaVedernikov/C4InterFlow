using System.CommandLine;
using C4InterFlow.Cli.Commands.Options;

namespace C4InterFlow.Cli.Root;

internal sealed class RootCommandContext: IRootCommandContext
{
    private readonly RootCommand? _root;
    
    public RootCommandContext()
    {
        _root = new RootCommand();
        
        _root.AddGlobalOption(LoggingOutputOptions.Get());
        _root.AddGlobalOption(LoggingLevelOptions.Get());
    }

    public RootCommand? GetRootCommand()
    {
        return _root;
    }

    public IRootCommandContext Add<TCommand>()
        where TCommand: Command
    {
        var command = Activator.CreateInstance<TCommand>();
        _root?.AddCommand(command);
        return this;
    }

    public async Task<int> InvokeAsync(string[] args) => _root is null 
        ? throw new SystemException("The RootCommand was not created! Please retry execute the command") 
        : await _root.InvokeAsync(args);
}
