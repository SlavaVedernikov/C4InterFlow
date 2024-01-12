using C4InterFlow.Elements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow.Elements.Boundaries
{
    public record PackageBoundary : Structure, IBoundary
    {
        public PackageBoundary(string alias, string label)
            : base(alias, label)
        {
        }
        public List<Entity> Entities { get; } = new List<Entity>();
        public Structure[] GetBoundaryStructures() => Entities.Select(x => x as Structure).ToArray();
    }
}
