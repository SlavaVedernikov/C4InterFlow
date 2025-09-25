using C4InterFlow.Structures.Boundaries;

namespace C4InterFlow.Structures;

internal class StructureCollection
{
    public IDictionary<string, Structure> Items { get; }

    public StructureCollection()
    {
        Items = new Dictionary<string, Structure>();
    }

    public void AddRange(IEnumerable<Structure> structures)
    {
        var enumerable = structures as Structure[] ?? structures.ToArray();
        if (!enumerable.Any())
        {
            return;
        }

        foreach (var structure in enumerable)
        {
            Add(structure);
        }
    }

    private void Add(Structure structure)
    {
        if (structure is IBoundary boundary)
        {
            foreach (var boundaryStructure in boundary.GetStructures())
            {
                Add(boundaryStructure);
            }
        }

        Items[structure.Alias] = structure;
    }
}