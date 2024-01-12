// See https://aka.ms/new-console-template for more information
using C4PlusSharp.Cli.Root;
using C4PlusSharp.Cli.Commands;

var root = RootCommandBuilder
    .CreateDefaultBuilder(args)
    .Configure(context =>
    {
        context.Add<DrawDiagramsCommand>();
        context.Add<QueryUseFlowsCommand>();
        context.Add<QueryByInputCommand>();
        context.Add<ExecuteArchitectureAsCodeStrategyCommand>();
    });

await root.Run();

Console.ReadLine();
