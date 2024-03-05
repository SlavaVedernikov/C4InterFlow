using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow.Structures
{
    public record StructureAttribute : Structure
    {
        public object? Value { get; init; }

        public StructureAttribute(string alias, string label, object? value) : base(alias, label)
        {
            Value = value;
        }
    }
}
