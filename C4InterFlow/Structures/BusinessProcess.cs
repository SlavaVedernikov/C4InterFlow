namespace C4InterFlow.Structures
{
    public record BusinessProcess : Structure
    {
        public BusinessProcess(IEnumerable<BusinessActivity> activities) : this(activities, string.Empty)
        {

        }

        public BusinessProcess(IEnumerable<BusinessActivity> activities, string label) : base(string.Empty, label)
        {
            Activities = activities;
        }

        public IEnumerable<BusinessActivity> Activities { get; init; }

    }
}
