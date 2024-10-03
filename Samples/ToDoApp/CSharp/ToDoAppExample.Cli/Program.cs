using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using C4InterFlow.Cli.Root;
using C4InterFlow.Cli.Commands;
using C4InterFlow.Cli.Extensions;

var rootCommandBuilder = RootCommandBuilder
    .CreateDefaultBuilder(args)
    .Configure(context =>
    {
        context.Add<DrawDiagramsCommand>();
    });

await new CommandLineBuilder(rootCommandBuilder.Build())
    .UseDefaults().UseLogging().Build().InvokeAsync(args);
