using C4InterFlow.Structures.Interfaces;
using System.Runtime.CompilerServices;

namespace C4InterFlow.Structures;

/// <summary>
/// Not Docker! In the C4 model, a container represents an application or a data store. A container is something
/// that needs to be running in order for the overall software system to work. In real terms, a container is
/// something like:
///
/// Server-side web application, Client-side web application, Client-side desktop application,
/// Mobile app, Server-side console application, Serverless function, Database, Blob or content store,
/// File system, Shell script
///
/// <see href="https://c4model.com/#ContainerDiagram"/>
/// </summary>
public record Container : Structure, ISupportBoundary
{
    public Boundary Boundary { get; init; } = Boundary.Internal;
    public ContainerType ContainerType { get; init; }
    public string? Technology { get; init; }
    public string SoftwareSystem { get; init; }
    public Container(Type type) : this(GetAlias(type), Utils.GetLabelFromAlias(GetAlias(type)))
    { }
    public Container(Type type, string label) : this(GetAlias(type), label)
    { }
    public Container(string alias) : this(alias, Utils.GetLabelFromAlias(alias))
    { }
    public Container(string alias, string label) : this(Utils.GetSoftwareSystemAlias(alias), alias, label)
    { }
    public Container(string system, string alias, string label) : base(alias, label)
    {
        SoftwareSystem = system;
        ContainerType = ContainerType.None;
    }

    public static string GetAlias<T>() where T : IContainerInstance
    {
        return Utils.GetStructureAlias<T>();
    }

    public static string GetAlias(Type type)
    {
        if (!typeof(IContainerInstance).IsAssignableFrom(type))
            throw new ArgumentException($"Expected '{typeof(IContainerInstance).FullName}' type, but '{type.FullName}' type provided.", nameof(type));

        return Utils.GetStructureAlias(type);
    }
}