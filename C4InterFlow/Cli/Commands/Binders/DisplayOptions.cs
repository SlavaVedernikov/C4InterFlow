using System.CommandLine.Binding;
using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Binders
{
    public class DisplayOptions
    {
        public DisplayOptions(bool showInterfaceInputAndOutput, int maxLineLabels, bool expandUpstream) {
            ShowBoundaries = true;
            ShowInterfaceInputAndOutput = showInterfaceInputAndOutput;
            MaxLineLabels = maxLineLabels;
            ExpandUpstream = expandUpstream;
        }
        public bool ShowBoundaries { get; private set; }
        public bool ShowInterfaceInputAndOutput { get; private set; }
        public int MaxLineLabels { get; private set; }
        public bool ExpandUpstream { get; private set; }
    }

    public class DisplayOptionsBinder : BinderBase<DisplayOptions>
    {
        private readonly Option<bool> _showInterfaceInputAndOutput;
        private readonly Option<int> _maxLineLabels;
        private readonly Option<bool> _expandUpstream;

        public DisplayOptionsBinder(Option<bool> showInterfaceInputAndOutput, Option<int> maxLineLabels, Option<bool> expandUpstream)
        {
            _showInterfaceInputAndOutput = showInterfaceInputAndOutput;
            _maxLineLabels = maxLineLabels;
            _expandUpstream = expandUpstream;
        }

        protected override DisplayOptions GetBoundValue(BindingContext bindingContext) =>
            new DisplayOptions(
                bindingContext.ParseResult.GetValueForOption(_showInterfaceInputAndOutput),
                bindingContext.ParseResult.GetValueForOption(_maxLineLabels),
                bindingContext.ParseResult.GetValueForOption(_expandUpstream));
    }
}
