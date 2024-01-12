namespace C4InterFlow.Elements.Boundaries;

public record EnterpriseBoundary(string Alias, string Label) : Structure(Alias, Label), IBoundary
{
    public IEnumerable<Structure> Structures { get; init; } = Array.Empty<Component>();
    public Structure[] GetBoundaryStructures() => Structures.Select(x => x).ToArray();

}
