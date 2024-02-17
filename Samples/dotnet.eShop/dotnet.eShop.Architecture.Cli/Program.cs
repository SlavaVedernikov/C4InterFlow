// See https://aka.ms/new-console-template for more information
using C4InterFlow.Cli;
using C4InterFlow.Cli.Root;
using C4InterFlow.Cli.Commands;
using C4InterFlow.Automation;

var root = RootCommandBuilder
    .CreateDefaultBuilder(args)
    .Configure(context =>
    {
        context.Add<DrawDiagramsCommand>();
        context.Add<QueryUseFlowsCommand>();
        context.Add<QueryByInputCommand>();
        context.Add<ExecuteAaCStrategyCommand>();
    });

await root.Run();