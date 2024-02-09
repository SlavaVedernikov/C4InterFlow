using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using C4InterFlow.Automation;
using C4InterFlow.Cli;
using C4InterFlow.Elements;
using C4InterFlow.Elements.Interfaces;
using C4InterFlow.Elements.Relationships;

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
