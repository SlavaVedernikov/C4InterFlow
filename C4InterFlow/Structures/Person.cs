using C4InterFlow.Structures.Interfaces;

namespace C4InterFlow.Structures;

/// <summary>
/// A person represents one of the human users of your software system (e.g. actors, roles, personas, etc)
/// <see href="https://c4model.com/"/>
/// </summary>
public sealed record Person : Structure, ISupportBoundary
{
    public Boundary Boundary { get; init; } = Boundary.Internal;
    public Person(string alias, string label) : base(alias, label)
    {
    }

    public Person(string alias, string label, string description) : this(alias, label)
    {
        Description = description;
    }

    public Person(string alias, string label, string description, Boundary boundary) : this(alias, label)
    {
        Description = description;
        Boundary = boundary;
    }
}