using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow
{
    public record Tags
    {
        public const string LIFECYCLE_CHANGED = "c4interflow:lifecycle:changed";
        public const string LIFECYCLE_NEW = "c4interflow:lifecycle:new";
        public const string LIFECYCLE_REMOVED = "c4interflow:lifecycle:removed";
    }
}
