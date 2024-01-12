using C4InterFlow.Elements;
using C4InterFlow.Elements.Interfaces;

namespace C4InterFlow
{
    public class BusinessProcess
    {
        public BusinessProcess(IEnumerable<BusinessActivity> activities)
        {
            Activities = activities;
        }

        public BusinessProcess(IEnumerable<BusinessActivity> activities, string label) : this(activities)
        {
            Label = label;
        }

        public string? Label { get; private set; }

        public IEnumerable<BusinessActivity> Activities { get; init; }

    }
}
