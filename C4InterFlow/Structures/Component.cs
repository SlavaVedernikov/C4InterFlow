using C4InterFlow.Structures.Interfaces;

namespace C4InterFlow.Structures;

/// <summary>
/// The word "component" is a hugely overloaded term in the software development industry, but in this context a
/// component is a grouping of related functionality encapsulated behind a well-defined interface. If you're using
/// a language like Java or C#, the simplest way to think of a component is that it's a collection of implementation
/// classes behind an interface. Aspects such as how those components are packaged (e.g. one component vs many
/// components per JAR file, DLL, shared library, etc) is a separate and orthogonal concern.
/// An important point to note here is that all components inside a container typically execute in the same process
/// space. In the C4 model, components are not separately deployable units.
/// <see href="https://c4model.com/"/>
/// </summary>
public record Component : Structure
{
    public string? Technology { get; init; }
    public ComponentType ComponentType { get; init; }
    public string Container { get; init; }
    public Note[]? Notes { get; init; }

    public Component(Type type) : this(GetAlias(type), Utils.GetLabelFromAlias(GetAlias(type)))
    { }
    public Component(Type type, string label) : this(GetAlias(type), label)
    { }
    public Component(string alias) : this(alias, Utils.GetLabelFromAlias(alias))
    { }
    public Component(string alias, string label) : this(Utils.GetContainerAlias(alias), alias, label)
    { }
    public Component(string container, string alias, string label) : base(alias, label)
    {
        Container = container;
        ComponentType = ComponentType.None;
    }

    public static string GetAlias<T>() where T : IComponentInstance
    {
        return Utils.GetStructureAlias<T>();
    }

    public static string GetAlias(Type type)
    {
        if (!typeof(IComponentInstance).IsAssignableFrom(type))
            throw new ArgumentException($"Expected '{typeof(IComponentInstance).FullName}' type, but '{type.FullName}' type provided.", nameof(type));

        return Utils.GetStructureAlias(type);
    }

}
