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
                            public partial class GetBasket : IInterfaceInstance
                            {
                                public Interface Instance => new Interface(GetType(), "Get Basket")
                                {
                                    Description = "",
                                    Path = "",
                                    IsPrivate = false,
                                    Protocol = "",
                                    Flow = new Flow(Interface.GetAlias(GetType()))
                                    	.Use<DotNetEShop.SoftwareSystems.BasketApi.Containers.Data.Components.RedisBasketRepository.Interfaces.GetBasketAsync>()
                                    	.If(@"data is not null")
                                    		.Use<DotNetEShop.SoftwareSystems.BasketApi.Containers.Grpc.Components.BasketService.Interfaces.MapToCustomerBasketResponse>()
                                    	.EndIf(),
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