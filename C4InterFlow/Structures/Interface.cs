namespace C4InterFlow.Structures
{
    public record Interface : Structure
    {
        public Interface(string alias) : this(alias, Utils.GetLabelFromAlias(alias))
        { }
        public Interface(string alias, string label) : this(Utils.GetInterfaceOwnerAlias(alias), alias, label)
        { }
        public Interface(string owner, string alias, string label) : base(alias, label)
        {
            Owner = owner;
        }

        public string Protocol { get; init; }
        public string Path { get; init; }
        public string Owner { get; init; }

        public Flow Flow { get; init; }

    }
}
