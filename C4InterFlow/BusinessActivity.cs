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
        public BusinessActivity(Flow flow) : this(flow, string.Empty) { }

        public BusinessActivity(Flow flow, string label)
        {
            Flow = flow;
            Label = label;
        }

        public Structure? Actor { 
            get 
            {
                return Utils.GetInstance<Structure>(Flow.OwnerAlias);
            }
        }
        public string? Label { get; private set; }
        public Flow Flow { get; private set; }
    }

}
