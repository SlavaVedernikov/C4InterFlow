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
            public partial class Infrastructure
            {
                public partial class Components
                {
                    public partial class CatalogContext
                    {
                        public partial class Interfaces
                        {
                            public partial class CatalogItemsAdd : IInterfaceInstance
                            {
                                public static Interface Instance => new Interface(Utils.GetStructureAlias<CatalogItemsAdd>(), "Catalog Items Add")
                                {
                                    Description = "",
                                    Path = "",
                                    IsPrivate = false,
                                    Protocol = "",
                                    Flow = new Flow(Utils.GetStructureAlias<CatalogItemsAdd>()),
                                    Input = "",
                                    InputTemplate = "",
                                    Output = "",
                                    OutputTemplate = ""
                                };
                            }
                        }
                    }
                }
            }
        }
    }
}