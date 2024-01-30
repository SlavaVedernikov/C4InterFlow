// <auto-generated/>
using C4InterFlow.Elements;
using C4InterFlow.Elements.Interfaces;
using C4InterFlow.Elements.Relationships;

namespace dotnet.eShop.Architecture.SoftwareSystems
{
    public partial class BasketApi
    {
        public partial class Containers
        {
            public partial class Data : IContainerInstance
            {
                public const string ALIAS = "dotnet.eShop.Architecture.SoftwareSystems.BasketApi.Containers.Data";
                public static Container Instance => new Container(dotnet.eShop.Architecture.SoftwareSystems.BasketApi.ALIAS, ALIAS, "Data")
                {
                    ContainerType = ContainerType.None,
                    Description = "",
                    Technology = "",
                    Boundary = Boundary.Internal
                };

                public partial class Components
                {
                }

                public partial class Interfaces
                {
                }

                public partial class Entities
                {
                }
            }
        }
    }
}