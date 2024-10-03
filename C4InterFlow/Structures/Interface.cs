using C4InterFlow.Structures.Interfaces;

namespace C4InterFlow.Structures
{
    public record Interface : Structure
    {
        public Interface(Type type) : this(GetAlias(type), Utils.GetLabelFromAlias(GetAlias(type)))
        { }
        public Interface(Type type, string label) : this(GetAlias(type), label)
        { }
        public Interface(string alias) : this(alias, Utils.GetLabelFromAlias(alias))
        { }
        public Interface(string alias, string label) : this(Utils.GetInterfaceOwnerAlias(alias), alias, label)
        { }
        public Interface(string owner, string alias, string label) : base(alias, label)
        {
            Owner = owner;
        }

        public static string GetAlias<T>() where T : IInterfaceInstance
        {
            return Utils.GetStructureAlias<T>();
        }

        public static string GetAlias(Type type)
        {
            if (!typeof(IInterfaceInstance).IsAssignableFrom(type))
                throw new ArgumentException($"Expected '{typeof(IInterfaceInstance).FullName}' type, but '{type.FullName}' type provided.", nameof(type));

            return Utils.GetStructureAlias(type);
        }

        public string Protocol { get; init; }
        public bool IsPrivate { get; init; }
        public string Path { get; init; }
        public string Owner { get; init; }

        public Flow Flow { get; init; }

        public string Input { get; init; }

        public string InputTemplate { get; init; }

        public string Output { get; init; }

        public string OutputTemplate { get; init; }

        public SoftwareSystem? GetSoftwareSystem()
        {
            var result = default(SoftwareSystem);

            var interfaceOwner = Utils.GetInstance<Structure>(Owner);

            if (interfaceOwner is SoftwareSystem)
            {
                result = (SoftwareSystem)interfaceOwner;
            }
            else if (interfaceOwner is Container)
            {
                var container = (Container)interfaceOwner;
                result = Utils.GetInstance<Structure>(container.SoftwareSystem) as SoftwareSystem; 
            }
            else if (interfaceOwner is Component)
            {
                var component = (Component)interfaceOwner;
                var container = Utils.GetInstance<Structure>(component.Container) as Container;
                result = Utils.GetInstance<Structure>(container.SoftwareSystem) as SoftwareSystem;
            }
            
            return result;
        }
    }
}
