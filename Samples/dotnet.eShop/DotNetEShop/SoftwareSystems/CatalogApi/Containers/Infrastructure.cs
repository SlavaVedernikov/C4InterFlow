// <auto-generated/>
using C4InterFlow;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;

namespace DotNetEShop.SoftwareSystems
{
    public partial class CatalogApi
    {
        public partial class Containers
        {
            public partial class Infrastructure : IContainerInstance
            {
                public static Container Instance => new Container(Utils.GetStructureAlias<Infrastructure>(), "Infrastructure")
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