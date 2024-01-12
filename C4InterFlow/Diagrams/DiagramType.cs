namespace C4InterFlow.Diagrams;

public record DiagramType(string Value, string Name, string SubType = "")
{
    public static DiagramType Component => new(DiagramConstants.Component, nameof(DiagramConstants.Component));
    public static DiagramType Container => new(DiagramConstants.Container, nameof(DiagramConstants.Container));
    public static DiagramType Context => new(DiagramConstants.Context, nameof(DiagramConstants.Context));
    public static DiagramType ComponentStatic => new(DiagramConstants.Component, nameof(DiagramConstants.Component), DiagramConstants.Static);
    public static DiagramType ContainerStatic => new(DiagramConstants.Container, nameof(DiagramConstants.Container), DiagramConstants.Static);
    public static DiagramType ContextStatic => new(DiagramConstants.Context, nameof(DiagramConstants.Context), DiagramConstants.Static);
    public static DiagramType Deployment => new(DiagramConstants.Deployment, nameof(DiagramConstants.Deployment));
}