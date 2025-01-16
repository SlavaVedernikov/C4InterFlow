using C4InterFlow.Structures.Interfaces;

namespace C4InterFlow.Structures;

/// <summary>
/// A software system is the highest level of abstraction and describes something that delivers value to its users,
/// whether they are human or not. This includes the software system you are modelling, and the other software
/// systems upon which your software system depends (or vice versa). In many cases, a software system is "owned by"
/// a single software development team.
/// <see href="https://c4model.com/"/>
/// </summary>
public sealed record SoftwareSystem : Structure
{
    public SoftwareSystem(Type type) : this(GetAlias(type), Utils.GetLabelFromAlias(GetAlias(type)))
    { }
    public SoftwareSystem(Type type, string label) : this(GetAlias(type), label)
    { }
    public SoftwareSystem(string alias) : this(alias, Utils.GetLabelFromAlias(alias))
    { }
    public SoftwareSystem(string alias, string label) : base(alias, label)
    { }

    public SoftwareSystem(string alias, string label, string description) : this(alias, label)
    {
        Description = description;
    }

    public SoftwareSystem(string alias, string label, string description, Boundary boundary) : this(alias, label)
    {
        Description = description;
        Boundary = boundary;
    }

    public static string GetAlias<T>() where T : ISoftwareSystemInstance
    {
        return Utils.GetStructureAlias<T>();
    }

    public static string GetAlias(Type type)
    {
        if (!typeof(ISoftwareSystemInstance).IsAssignableFrom(type))
            throw new ArgumentException($"Expected '{typeof(ISoftwareSystemInstance).FullName}' type, but '{type.FullName}' type provided.", nameof(type));

        return Utils.GetStructureAlias(type);
    }
}
