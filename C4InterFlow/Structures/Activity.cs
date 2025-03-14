﻿namespace C4InterFlow.Structures
{
    public record Activity
    {
        public Activity(Flow flow, string actor, string? label = null) : this(flow.Flows.ToArray(), actor, label) { }

        public Activity(Flow[] flows, string actor, string? label = null)
        {
            Actor = actor;
            Flows = flows;
            Label = label;
        }
        public string? Label { get; private set; }

        private Flow _flow;
        public Flow Flow {
            get
            {
                return _flow;
            }
            private set {
                _flow = value;
                SetFlowOwner();
            }
        }

        public Flow[] Flows {
            get
            {
                return Flow?.Flows?.ToArray();
            }
            private set
            {
                if (Flow == null) { Flow = new Flow(); }
                Flow.Flows = value;
                SetFlowOwner();
            }
        }

        private string _actor;
        public string Actor {
            get {
                return _actor;
            }
            private set
            {
                _actor = value;
                SetFlowOwner();
            }
        }

        private void SetFlowOwner()
        {
            if (Flow != null &&
                string.IsNullOrEmpty(Flow.Owner) &&
                !string.IsNullOrEmpty(Actor))
            {
                Flow.Owner = Actor;
            }
        }
        public Structure? GetActorInstance()
        {
            return Utils.GetInstance<Structure>(Actor);
        }
    }

}
