using C4InterFlow.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow
{
    public record BusinessActivity
    {
        public BusinessActivity(Flow flow, string actor, string? label = null)
        {
            Actor = actor;

            Flow = flow;

            if(string.IsNullOrEmpty(Flow.Owner))
            {
                Flow.Owner = Actor;
            }

            Label = label;
        }
        public string? Label { get; private set; }
        public Flow Flow { get; private set; }
        public string Actor { get; private set; }

        public Structure? GetActorInstance() {
            return Utils.GetInstance<Structure>(Actor);
        }
    }

}
