using C4InterFlow.Elements;

namespace C4InterFlow.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityAttribute : Attribute
    {
        public EntityAttribute(string container)
        {
            Container = container;
        }

        public string? Alias { get; set; }

        public string? Label { get; set; }

        public string? Description { get; set; }

        public EntityType Type { get; set; }

        public string[]? ComposedOfMany { get; set; }

        public string[]? ComposedOfOne { get; set; }

        public string? Extends { get; set; }

        public string Container { get; set; }
    }
}
