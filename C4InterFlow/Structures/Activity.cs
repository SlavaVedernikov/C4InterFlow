namespace C4InterFlow.Structures
{
    public record Activity
    {
        public Activity(Flow flow, string actor, string? label = null)
        {
            Actor = actor;

            Flow = flow;

            if (string.IsNullOrEmpty(Flow.Owner))
            {
                Flow.Owner = Actor;
            }

            Label = label;
        }
        public string? Label { get; private set; }
        public Flow Flow { get; private set; }
        public string Actor { get; private set; }

        public Structure? GetActorInstance()
        {
            return Utils.GetInstance<Structure>(Actor);
        }
    }

}
