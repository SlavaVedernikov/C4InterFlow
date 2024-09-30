// See https://aka.ms/new-console-template for more information

using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Runtime.CompilerServices;
using C4InterFlow.Cli.Commands;
using C4InterFlow.Cli.Extensions;
using C4InterFlow.Cli.Root;

[assembly:InternalsVisibleTo("C4InterFlow.Specs")]

namespace C4InterFlow.Cli;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommandBuilder = RootCommandBuilder
            .CreateDefaultBuilder(args)
            .Configure(context =>
            {
                context.Add<DrawDiagramsCommand>();
                context.Add<QueryUseFlowsCommand>();
                context.Add<QueryByInputCommand>();
                context.Add<ExecuteAaCStrategyCommand>();
                context.Add<GenerateDocumentationCommand>();
                context.Add<PublishSiteCommand>();
            });

        return await new CommandLineBuilder(rootCommandBuilder.Build())
            .UseDefaults().UseLogging().Build().InvokeAsync(args);
    }
}