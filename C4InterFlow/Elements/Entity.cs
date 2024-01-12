using System.ComponentModel;

namespace C4InterFlow.Elements
{
    public enum EntityType
    {
        [Description("None")]
        None,
        [Description("Enum")]
        Enum,
        [Description("Data")]
        Data,
        [Description("Message")]
        Message,
        [Description("Query")]
        Query,
        [Description("Command")]
        Command,
        [Description("Event")]
        Event
    }

    public record Entity : Structure
    {
        public Entity(string container, string alias, string label, EntityType entityType)
            : base(alias, label)
        {
            EntityType = entityType;
            Container = container;
        }
        public EntityType EntityType { get; init; }
        public string Container { get; init; }
        public string[] ComposedOfMany { get; init; }
        public string[] ComposedOfOne { get; init; }
        public string Extends { get; init; }
    }
}
