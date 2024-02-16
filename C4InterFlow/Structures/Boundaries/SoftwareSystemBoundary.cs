namespace C4InterFlow.Structures.Boundaries;

/// <summary>
/// Software System Boundary
/// </summary>
public sealed record SoftwareSystemBoundary(string Alias, string Label) : Structure(Alias, Label), IBoundary
{
    public IEnumerable<Structure> Structures { get; init; } = Array.Empty<Structure>();
    public Structure[] GetBoundaryStructures() => Structures.Select(x => x).ToArray();
}

