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
public record Container : Structure
{
    public ContainerType ContainerType { get; init; }
    public string? Technology { get; init; }
    public string SoftwareSystem { get; init; }

    public Container(string system, string alias, string label) : base(alias, label)
    {
        SoftwareSystem = system;
        ContainerType = ContainerType.None;
    }
}