using C4InterFlow.Elements;
using C4InterFlow.Elements.Interfaces;

namespace C4InterFlow
{
    public record BusinessProcess : Structure
    {
        public BusinessProcess(IEnumerable<BusinessActivity> activities): this(activities, string.Empty)
        {
            
        }

        public BusinessProcess(IEnumerable<BusinessActivity> activities, string label) : base(string.Empty, label)
        {
            Activities = activities;
        }

        public IEnumerable<BusinessActivity> Activities { get; init; }

    }
}
