using C4InterFlow.Structures;
using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine;
using C4InterFlow.Cli.Commands.Options;

namespace C4InterFlow.Cli.Commands.Binders
{
    public class OutputOptions
    {
        public OutputOptions(string outputDirectory, string? diagramNamePrefix, string[]? formats, string? outputSubDirectory = null, string? subtractPath = null) { 
            OutputDirectory = Path.GetFullPath(outputDirectory);
            DiagramNamePrefix = diagramNamePrefix;
            Formats = formats;
            OutputSubDirectory = outputSubDirectory;
            SubtractPath = subtractPath;
        }
        public string OutputDirectory { get; private set; }
        public string? OutputSubDirectory { get; private set; }
        public string? SubtractPath { get; private set; }
        public string? DiagramNamePrefix { get; private set; }
        public string[]? Formats { get; private set; }
    }

    public class OutputOptionsBinder : BinderBase<OutputOptions>
    {
        private readonly Option<string> _outputDirectoryOption;
        private readonly Option<string> _diagramNamePrefixOption;
        private readonly Option<string[]> _formatsOption;

        public OutputOptionsBinder(Option<string> outputDirectoryOption, Option<string> diagramNamePrefixOption, Option<string[]> formatsOption)
        {
            _outputDirectoryOption = outputDirectoryOption;
            _diagramNamePrefixOption = diagramNamePrefixOption;
            _formatsOption = formatsOption;
        }

        protected override OutputOptions GetBoundValue(BindingContext bindingContext) =>
            new OutputOptions(
                bindingContext.ParseResult.GetValueForOption(_outputDirectoryOption),
                bindingContext.ParseResult.GetValueForOption(_diagramNamePrefixOption),
                bindingContext.ParseResult.GetValueForOption(_formatsOption));
    }
}
