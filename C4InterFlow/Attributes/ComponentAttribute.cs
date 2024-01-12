using System;
using System.Collections.Generic;
using System.Text;
using C4InterFlow.Elements;

namespace C4InterFlow.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class ComponentAttribute : Attribute
    {
        public ComponentAttribute(string container)
        {
            Container = container;
        }
        public string? Alias { get; set; }

        public string? Label { get; set; }

        public string? Description { get; set; }

        public ComponentType Type { get; set; }

        public string? Technology { get; set; }

        public string Container { get; set; }
    }
}
