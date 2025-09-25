using Parlot.Fluent;

namespace C4InterFlow.Structures.Boundaries;

/// <summary>
/// Software System Boundary
/// </summary>
public sealed record SoftwareSystemBoundary(string Alias, string Label) : Structure(Alias, Label), IBoundary
{
    public IEnumerable<Structure> Structures { get; init; } = Array.Empty<Structure>();
    public Structure[] GetStructures(bool recursive = false)
    {
        if (recursive)
        {
            var result = new List<Structure>(Structures.OfType<Container>());

            foreach (var item in Structures.OfType<IBoundary>())
            {
                result.AddRange(item.GetStructures(true));
            }

            return result.ToArray();
        }
        else
        {
            return Structures.Select(x => x).ToArray();
        } 
    }
}

