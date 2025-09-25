namespace C4InterFlow.Structures.Boundaries;

public interface IBoundary
{
    Structure[] GetStructures(bool recursive = false);
}