using C4InterFlow.Structures;

namespace C4InterFlow.SoftwareSystems
{
    public class ExternalSystem
    {
        public const string ALIAS = $"{nameof(C4InterFlow)}.{nameof(SoftwareSystems)}.{nameof(ExternalSystem)}";
        public SoftwareSystem Instance => new SoftwareSystem(ALIAS, "External")
        {
            Boundary = Boundary.External
        };

        public class Interfaces
        {
            public class ExternalInterface
            {
                public const string ALIAS = $"{nameof(C4InterFlow)}.{nameof(SoftwareSystems)}.{nameof(ExternalSystem)}.{nameof(Interfaces)}.{nameof(ExternalInterface)}";
                public Interface Instance => new Interface(
                ExternalSystem.ALIAS,
                ALIAS,
                "External")
                {
                    Flow = new Flow(ALIAS)
                };
            }
        }
    }
}
