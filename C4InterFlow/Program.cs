// See https://aka.ms/new-console-template for more information

using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using C4InterFlow.Cli.Commands;
using C4InterFlow.Cli.Extensions;
using C4InterFlow.Cli.Root;

var rootCommandBuilder = RootCommandBuilder
    .CreateDefaultBuilder(args)
    .Configure(context =>
    {
        context.Add<DrawDiagramsCommand>();
        context.Add<QueryUseFlowsCommand>();
        context.Add<ExecuteViewsCommand>();
        context.Add<ExecuteCommandsCommand>();
        context.Add<ExportSchemaCommand>();
        context.Add<GenerateDocumentationCommand>();
    });

await new CommandLineBuilder(rootCommandBuilder.Build())
    .UseDefaults().UseLogging().Build().InvokeAsync(args);