using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow
{
    public record Tags
    {
        public const string STATE_EXISTING = "state:existing";
        public const string STATE_CHANGED = "state:changed";
        public const string STATE_NEW = "state:new";
        public const string STATE_REMOVED = "state:removed";
    }
}
