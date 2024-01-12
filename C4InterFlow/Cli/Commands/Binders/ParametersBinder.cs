using System;
using System.CommandLine.Binding;
using System.CommandLine;
using static System.Formats.Asn1.AsnWriter;

namespace C4InterFlow.Cli.Commands.Binders
{
    public class ParametersBinder : BinderBase<Dictionary<string, string>>
    {
        private readonly Option<string[]> _parameters;

        public ParametersBinder(Option<string[]> parameters)
        {
            _parameters = parameters;
        }

        protected override Dictionary<string, string> GetBoundValue(BindingContext bindingContext) =>
            bindingContext.ParseResult.GetValueForOption(_parameters)
            .Select(parameter => parameter.Split('='))
            .ToDictionary(kv => kv[0], kv => kv.Length > 1 ? kv[1] : null);
    }
}
