using C4InterFlow.Structures.Interfaces;
using System.ComponentModel;

namespace C4InterFlow.Structures
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
        public Entity(Type type, EntityType entityType) : this(GetAlias(type), Utils.GetLabelFromAlias(GetAlias(type)), entityType)
        { }
        public Entity(Type type, string label, EntityType entityType) : this(GetAlias(type), label, entityType)
        { }
        public Entity(string alias, EntityType entityType) : this(alias, Utils.GetLabelFromAlias(alias), entityType)
        { }
        public Entity(string alias, string label, EntityType entityType) : this(Utils.GetContainerAlias(alias), alias, label, entityType)
        { }
        public Entity(string container, string alias, string label, EntityType entityType)
            : base(alias, label)
        {
            EntityType = entityType;
            Container = container;
        }

        public static string GetAlias<T>() where T : IEntityInstance
        {
            return Utils.GetStructureAlias<T>();
        }

        public static string GetAlias(Type type)
        {
            if (!typeof(IEntityInstance).IsAssignableFrom(type))
                throw new ArgumentException($"Expected '{typeof(IEntityInstance).FullName}' type, but '{type.FullName}' type provided.", nameof(type));

            return Utils.GetStructureAlias(type);
        }

        public EntityType EntityType { get; init; }
        public string Container { get; init; }
        public string[] ComposedOfMany { get; init; }
        public string[] ComposedOfOne { get; init; }
        public string Extends { get; init; }
    }
}
