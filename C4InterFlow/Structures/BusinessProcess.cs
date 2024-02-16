namespace C4InterFlow.Structures
{
    public record BusinessProcess : Structure
    {
        public BusinessProcess(IEnumerable<Activity> activities) : this(activities, string.Empty)
        {

        }

        public BusinessProcess(IEnumerable<Activity> activities, string label) : base(string.Empty, label)
        {
            Activities = activities;
        }

        public IEnumerable<Activity> Activities { get; init; }

    }
}
