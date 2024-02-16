using System.ComponentModel;

namespace C4InterFlow.Structures;

/// <summary>
/// Component types
/// </summary>
public enum ComponentType
{
    [Description("")]
    None,

    [Description("Database")]
    Database,

    [Description("Queue")]
    Queue,

    [Description("Topic")]
    Topic,

    [Description("Topic Subscription")]
    TopicSubscription
}