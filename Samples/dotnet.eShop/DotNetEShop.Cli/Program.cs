// See https://aka.ms/new-console-template for more information

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
        context.Add<QueryUseFlowsCommand>();
        context.Add<QueryByInputCommand>();
        context.Add<ExecuteAaCStrategyCommand>();
    });

await new CommandLineBuilder(rootCommandBuilder.Build())
    .UseDefaults().UseLogging().Build().InvokeAsync(args);