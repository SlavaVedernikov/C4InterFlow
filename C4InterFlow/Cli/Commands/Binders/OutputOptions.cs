using C4InterFlow.Elements;
using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine;
using C4InterFlow.Cli.Commands.Options;

namespace C4InterFlow.Cli.Commands.Binders
{
    public class OutputOptions
    {
        public OutputOptions(string outputDirectory, string? outputSubDirectory, string? diagramNamePrefix, string[]? formats) { 
            OutputDirectory = outputDirectory;
            OutputSubDirectory = outputSubDirectory;
            DiagramNamePrefix = diagramNamePrefix;
            Formats = formats;
        }
        public string OutputDirectory { get; private set; }
        public string? OutputSubDirectory { get; private set; }
        public string? DiagramNamePrefix { get; private set; }
        public string[]? Formats { get; private set; }
    }

    public class OutputOptionsBinder : BinderBase<OutputOptions>
    {
        private readonly Option<string> _outputDirectoryOption;
        private readonly Option<string> _outputSubDirectoryOption;
        private readonly Option<string> _diagramNamePrefixOption;
        private readonly Option<string[]> _formatsOption;

        public OutputOptionsBinder(Option<string> outputDirectoryOption, Option<string> outputSubDirectoryOption, Option<string> diagramNamePrefixOption, Option<string[]> formatsOption)
        {
            _outputDirectoryOption = outputDirectoryOption;
            _outputSubDirectoryOption = outputSubDirectoryOption;
            _diagramNamePrefixOption = diagramNamePrefixOption;
            _formatsOption = formatsOption;
        }

        protected override OutputOptions GetBoundValue(BindingContext bindingContext) =>
            new OutputOptions(
                bindingContext.ParseResult.GetValueForOption(_outputDirectoryOption),
                bindingContext.ParseResult.GetValueForOption(_outputSubDirectoryOption),
                bindingContext.ParseResult.GetValueForOption(_diagramNamePrefixOption),
                bindingContext.ParseResult.GetValueForOption(_formatsOption));
    }
}
