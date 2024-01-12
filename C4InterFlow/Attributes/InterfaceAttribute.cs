using System;
using System.Collections.Generic;
using System.Text;

namespace C4InterFlow.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class InterfaceAttribute : Attribute
    {
        public InterfaceAttribute(string owner)
        {
            Owner = owner;
        }

        public string? Alias { get; set; }
        public string? Label { get; set; }
        public string? Description { get; set; }
        public string? Protocol { get; init; }
        public string Owner { get; init; }
        public string[]? Uses { get; init; }
        public string? Input { get; set; }
        public string? InputTemplate { get; set; }
        public string? Output { get; set; }
        public string? OutputTemplate { get; set; }
    }
}
