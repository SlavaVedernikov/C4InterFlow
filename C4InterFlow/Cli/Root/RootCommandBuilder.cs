using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace C4InterFlow.Cli.Root;

public class RootCommandBuilder 
{
    private readonly string[] _args;
    private readonly IRootCommandContext _context;

    private RootCommandBuilder(IRootCommandContext context, string[] args)
    {
        _context = context;
        _args = args;
    }

    public static RootCommandBuilder CreateDefaultBuilder(string[] args)
    {
        var builder = new RootCommandBuilder(new RootCommandContext(), args);
        
        return builder;
    }

    public RootCommandBuilder Configure<TStartup>() where TStartup : class
    {
        var startup = Activator.CreateInstance<TStartup>() as dynamic;
        startup?.Configuration(_context);
        return this;
    }

    public RootCommandBuilder Configure(Action<IRootCommandContext> action)
    {
        action.Invoke(_context);
        return this;
    }

    public RootCommand Build()
    {
        return _context.GetRootCommand();
    }
}