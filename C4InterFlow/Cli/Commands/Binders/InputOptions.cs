using C4InterFlow.Structures;
using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine;
using C4InterFlow.Cli.Commands.Options;

namespace C4InterFlow.Cli.Commands.Binders
{
    public class InputOptions
    {
        public InputOptions(string[] interfaces, string interfacesInputFile, string[] businessProcesses, string[] namespaces)
        {
            Interfaces = interfaces;
            InterfacesInputFile = interfacesInputFile;
            BusinessProcesses = businessProcesses;
            Namespaces = namespaces;
        }
        public string[] Interfaces { get; private set; }

        public string InterfacesInputFile { get; private set; }

        public string[] BusinessProcesses { get; private set; }

        public string[] Namespaces { get; private set; }
    }

    public class InputOptionsBinder : BinderBase<InputOptions>
    {
        private readonly Option<string[]> _interfacesOption;
        private readonly Option<string> _interfacesInputFileOption;
        private readonly Option<string[]> _businessProcessesOption;
        private readonly Option<string[]> _namespaces;

        public InputOptionsBinder(Option<string[]> interfacesOption, Option<string> interfacesInputFileOption, Option<string[]> businessProcessesOption, Option<string[]> namespaces)
        {
            _interfacesOption = interfacesOption;
            _interfacesInputFileOption = interfacesInputFileOption;
            _businessProcessesOption = businessProcessesOption;
            _namespaces = namespaces;
        }

        protected override InputOptions GetBoundValue(BindingContext bindingContext) =>
            new InputOptions(
                bindingContext.ParseResult.GetValueForOption(_interfacesOption),
                bindingContext.ParseResult.GetValueForOption(_interfacesInputFileOption),
                bindingContext.ParseResult.GetValueForOption(_businessProcessesOption),
                bindingContext.ParseResult.GetValueForOption(_namespaces));
    }
}
