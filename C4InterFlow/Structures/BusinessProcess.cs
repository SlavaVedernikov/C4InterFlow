namespace C4InterFlow.Structures
{
    public record BusinessProcess : Structure
    {
        public BusinessProcess(IEnumerable<Activity> activities) : this(activities, string.Empty)
        {

        }

        public BusinessProcess(IEnumerable<Activity> activities, string label) : this(activities, string.Empty, label)
        {

        }

        public BusinessProcess(IEnumerable<Activity> activities, string alias, string label) : base(alias, label)
        {
            Activities = activities;
        }

        public IEnumerable<Activity> Activities { get; init; }

    }
}
