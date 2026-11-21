using C4InterFlow.Structures.Relationships;

namespace C4InterFlow.Structures.Boundaries;

/// <summary>
/// Container Boundary
/// </summary>
public record ContainerBoundary : Structure, IBoundary
{
    public ContainerBoundary(string alias, string label) : base(alias, label)
    {
    }

    public ContainerBoundary(StructureIdentity alias, string label) : base(alias, label)
    {
    }

    public IEnumerable<Component> Components { get; init; } = Array.Empty<Component>();
    public Structure[] GetStructures(bool recursive = false) => Components.Select(x => x as Structure).ToArray();
}

