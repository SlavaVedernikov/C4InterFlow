using System.CommandLine.Binding;
using System.CommandLine;

namespace C4InterFlow.Cli.Commands.Binders
{
    public class DisplayOptions
    {
        public DisplayOptions(int maxLineLabels, bool expandUpstream) {
            ShowBoundaries = true;
            MaxLineLabels = maxLineLabels;
            ExpandUpstream = expandUpstream;
        }
        public bool ShowBoundaries { get; private set; }
        public int MaxLineLabels { get; private set; }
        public bool ExpandUpstream { get; private set; }
    }

    public class DisplayOptionsBinder : BinderBase<DisplayOptions>
    {
        private readonly Option<int> _maxLineLabels;
        private readonly Option<bool> _expandUpstream;

        public DisplayOptionsBinder(Option<int> maxLineLabels, Option<bool> expandUpstream)
        {
            _maxLineLabels = maxLineLabels;
            _expandUpstream = expandUpstream;
        }

        protected override DisplayOptions GetBoundValue(BindingContext bindingContext) =>
            new DisplayOptions(
                bindingContext.ParseResult.GetValueForOption(_maxLineLabels),
                bindingContext.ParseResult.GetValueForOption(_expandUpstream));
    }
}
