using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;

namespace C4InterFlow.SoftwareSystems
{
    public class ExternalSystem : ISoftwareSystemInstance
    {
        public const string ALIAS = $"{nameof(C4InterFlow)}.{nameof(SoftwareSystems)}.{nameof(ExternalSystem)}";
        public static SoftwareSystem Instance => new SoftwareSystem(ALIAS, "External")
        {
            Boundary = Boundary.External
        };

        public class Interfaces
        {
            public class ExternalInterface : IInterfaceInstance
            {
                public const string ALIAS = $"{nameof(C4InterFlow)}{nameof(SoftwareSystems)}.{nameof(ExternalSystem)}.{nameof(Interfaces)}.{nameof(ExternalInterface)}";
                public static Interface Instance => new Interface(
                ExternalSystem.ALIAS,
                ALIAS,
                "External");
            }
        }
    }
}
