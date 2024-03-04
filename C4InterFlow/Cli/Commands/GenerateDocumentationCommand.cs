using System.CommandLine;
using C4InterFlow.Structures;
using C4InterFlow.Cli.Commands.Options;
using C4InterFlow.Automation;
using System.Reflection;
using C4InterFlow.Cli.Commands.Binders;

namespace C4InterFlow.Cli.Commands;

public class ExecuteAaCStrategyCommand : Command
{
    private const string COMMAND_NAME = "execute-aac-strategy";
    public ExecuteAaCStrategyCommand() : base(COMMAND_NAME,
        "Executes Architecture As Code generation Strategy")
    {
        var structuresOption = StructuresOption.Get();
        var documentationTemplateFileOption = DocumentationTemplateFileOption.Get();
        var outputDirectoryOption = OutputDirectoryOption.Get();
        var fileExtensionOption = FileExtensionOption.Get();
        var architectureAsCodeInputPathsOption = AaCInputPathsOption.Get();
        var architectureAsCodeReaderStrategyTypeOption = AaCReaderStrategyTypeOption.Get();

        AddOption(structuresOption);
        AddOption(documentationTemplateFileOption);
        AddOption(outputDirectoryOption);
        AddOption(fileExtensionOption);
        AddOption(architectureAsCodeInputPathsOption);
        AddOption(architectureAsCodeReaderStrategyTypeOption);

        this.SetHandler(async (structures, documentationTemplateFile, outputDirectory, fileExtension, architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType) =>
            {
                await Execute(structures, documentationTemplateFile, outputDirectory, fileExtension, architectureAsCodeInputPaths, architectureAsCodeReaderStrategyType);
            },
            structuresOption, documentationTemplateFileOption, outputDirectoryOption, fileExtensionOption, architectureAsCodeInputPathsOption, architectureAsCodeReaderStrategyTypeOption);
    }

    private static async Task<int> Execute(string structures, string documentationTemplateFile, string outputDirectory, string fileExtension, string[] architectureAsCodeInputPaths, string architectureAsCodeReaderStrategyType)
    {
        try
        {
            Console.WriteLine($"'{COMMAND_NAME}' command is executing...");

            
            
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Documentation generation failed with exception(s) '{e.Message}'{(e.InnerException != null ? $", '{e.InnerException}'" : string.Empty)}.");
            return 1;
        }
    }
    
}
