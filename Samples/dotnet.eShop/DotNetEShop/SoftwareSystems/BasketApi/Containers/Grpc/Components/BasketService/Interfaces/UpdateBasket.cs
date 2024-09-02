// <auto-generated/>
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
                            public partial class UpdateBasket : IInterfaceInstance
                            {
                                private static readonly string ALIAS = "DotNetEShop.SoftwareSystems.BasketApi.Containers.Grpc.Components.BasketService.Interfaces.UpdateBasket";
                                public static Interface Instance => new Interface(DotNetEShop.SoftwareSystems.BasketApi.Containers.Grpc.Components.BasketService.ALIAS, ALIAS, "Update Basket")
                                {
                                    Description = "",
                                    Path = "",
                                    IsPrivate = false,
                                    Protocol = "",
                                    Flow = new Flow(ALIAS)
                                    	.If(@"string.IsNullOrEmpty(userId)")
                                    		.Use("DotNetEShop.SoftwareSystems.BasketApi.Containers.Grpc.Components.BasketService.Interfaces.ThrowNotAuthenticated")
                                    	.EndIf()
                                    	.Use("DotNetEShop.SoftwareSystems.BasketApi.Containers.Grpc.Components.BasketService.Interfaces.MapToCustomerBasket")
                                    	.Use("DotNetEShop.SoftwareSystems.BasketApi.Containers.Data.Components.RedisBasketRepository.Interfaces.UpdateBasketAsync")
                                    	.If(@"response is null")
                                    		.Use("DotNetEShop.SoftwareSystems.BasketApi.Containers.Grpc.Components.BasketService.Interfaces.ThrowBasketDoesNotExist")
                                    	.EndIf()
                                    	.Use("DotNetEShop.SoftwareSystems.BasketApi.Containers.Grpc.Components.BasketService.Interfaces.MapToCustomerBasketResponse"),
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