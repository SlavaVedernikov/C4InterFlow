// <auto-generated/>
using C4InterFlow;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;

namespace DotNetEShop.SoftwareSystems
{
    public partial class BasketApi
    {
        public partial class Containers
        {
            public partial class Grpc
            {
                public partial class Components
                {
                    public partial class BasketService
                    {
                        public partial class Interfaces
                        {
                            public partial class MapToCustomerBasket : IInterfaceInstance
                            {
                                public static Interface Instance => new Interface(Utils.GetStructureAlias<MapToCustomerBasket>(), "Map To Customer Basket")
                                {
                                    Description = "",
                                    Path = "",
                                    IsPrivate = true,
                                    Protocol = "",
                                    Flow = new Flow(Utils.GetStructureAlias<MapToCustomerBasket>())
                                    	.Return(@"response"),
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