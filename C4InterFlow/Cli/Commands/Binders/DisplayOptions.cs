using System.CommandLine.Binding;
using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Binders
{
    public class DisplayOptions
    {
        public DisplayOptions(bool? showBoundaries, bool showInterfaceInputAndOutput) {
            ShowBoundaries = showBoundaries.HasValue ? showBoundaries.Value : true;
            ShowInterfaceInputAndOutput = showInterfaceInputAndOutput;
        }
        public bool ShowBoundaries { get; private set; }
        public bool ShowInterfaceInputAndOutput { get; private set; }
    }

    public class DisplayOptionsBinder : BinderBase<DisplayOptions>
    {
        private readonly Option<bool?> _showBoundaries;
        private readonly Option<bool> _showInterfaceInputAndOutput;

        public DisplayOptionsBinder(Option<bool?> showBoundaries, Option<bool> showInterfaceInputAndOutput)
        {
            _showBoundaries = showBoundaries;
            _showInterfaceInputAndOutput = showInterfaceInputAndOutput;
        }

        protected override DisplayOptions GetBoundValue(BindingContext bindingContext) =>
            new DisplayOptions(
                bindingContext.ParseResult.GetValueForOption(_showBoundaries),
                bindingContext.ParseResult.GetValueForOption(_showInterfaceInputAndOutput));
    }
}
