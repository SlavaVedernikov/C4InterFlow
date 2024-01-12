using C4InterFlow.Elements;
using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Binders
{
    public class OutputOptions
    {
        public OutputOptions(string outputDirectory, bool clearOutputDirectory, string[] formats) { 
            OutputDirectory = outputDirectory;
            ClearOutputDirectory = clearOutputDirectory;
            Formats = formats;
        }
        public string OutputDirectory { get; private set; }
        public bool ClearOutputDirectory { get; private set; }
        public string[] Formats { get; private set; }
    }

    public class OutputOptionsBinder : BinderBase<OutputOptions>
    {
        private readonly Option<string> _outputDirectoryOption;
        private readonly Option<bool> _clearOutputDirectory;
        private readonly Option<string[]> _formats;

        public OutputOptionsBinder(Option<string> outputDirectoryOption, Option<bool> clearOutputDirectory, Option<string[]> formats)
        {
            _outputDirectoryOption = outputDirectoryOption;
            _clearOutputDirectory = clearOutputDirectory;
            _formats = formats;
        }

        protected override OutputOptions GetBoundValue(BindingContext bindingContext) =>
            new OutputOptions(
                bindingContext.ParseResult.GetValueForOption(_outputDirectoryOption),
                bindingContext.ParseResult.GetValueForOption(_clearOutputDirectory),
                bindingContext.ParseResult.GetValueForOption(_formats));
    }
}
