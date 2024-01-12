using C4InterFlow.Elements;
using C4InterFlow.Elements.Relationships;

namespace C4InterFlow.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ContainerAttribute : Attribute
    {
        public ContainerAttribute(string system)
        {
            System = system;
        }
        public string? Alias { get; init; }

        public string? Label { get; init; }

        public string? Description { get; init; }

        public ContainerType Type { get; init; }

        public string? Technology { get; init; }

        public string System { get; init; }

        public Boundary Boundary { get; init; }
    }
}
