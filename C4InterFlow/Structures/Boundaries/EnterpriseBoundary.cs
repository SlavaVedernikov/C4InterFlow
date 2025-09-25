namespace C4InterFlow.Structures.Boundaries;

public record EnterpriseBoundary(string Alias, string Label) : Structure(Alias, Label), IBoundary
{
    public IEnumerable<Structure> Structures { get; init; } = Array.Empty<Component>();
    public Structure[] GetStructures(bool recursive = false)
    {
        if (recursive)
        {
            var result = new List<Structure>(Structures.OfType<SoftwareSystem>());

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
