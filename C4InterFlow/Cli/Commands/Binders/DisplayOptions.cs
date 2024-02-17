using System.CommandLine.Binding;
using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Binders
{
    public class DisplayOptions
    {
        public DisplayOptions(bool showInterfaceInputAndOutput) {
            ShowBoundaries = true;
            ShowInterfaceInputAndOutput = showInterfaceInputAndOutput;
        }
        public bool ShowBoundaries { get; private set; }
        public bool ShowInterfaceInputAndOutput { get; private set; }
    }

    public class DisplayOptionsBinder : BinderBase<DisplayOptions>
    {
        private readonly Option<bool> _showInterfaceInputAndOutput;

        public DisplayOptionsBinder(Option<bool> showInterfaceInputAndOutput)
        {
            _showInterfaceInputAndOutput = showInterfaceInputAndOutput;
        }

        protected override DisplayOptions GetBoundValue(BindingContext bindingContext) =>
            new DisplayOptions(
                bindingContext.ParseResult.GetValueForOption(_showInterfaceInputAndOutput));
    }
}
