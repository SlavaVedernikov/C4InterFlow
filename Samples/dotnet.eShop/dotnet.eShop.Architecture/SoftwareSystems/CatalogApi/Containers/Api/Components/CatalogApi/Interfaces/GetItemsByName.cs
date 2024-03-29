// <auto-generated/>
using C4InterFlow.Structures;
using C4InterFlow.Structures.Interfaces;

namespace dotnet.eShop.Architecture.SoftwareSystems
{
    public partial class CatalogApi
    {
        public partial class Containers
        {
            public partial class Api
            {
                public partial class Components
                {
                    public partial class CatalogApi
                    {
                        public partial class Interfaces
                        {
                            public partial class GetItemsByName : IInterfaceInstance
                            {
                                private static readonly string ALIAS = "dotnet.eShop.Architecture.SoftwareSystems.CatalogApi.Containers.Api.Components.CatalogApi.Interfaces.GetItemsByName";
                                public static Interface Instance => new Interface(dotnet.eShop.Architecture.SoftwareSystems.CatalogApi.Containers.Api.Components.CatalogApi.ALIAS, ALIAS, "Get Items By Name")
                                {
                                    Description = "",
                                    Path = "",
                                    IsPrivate = false,
                                    Protocol = "",
                                    Flow = new Flow(ALIAS)
                                    	.Use("dotnet.eShop.Architecture.SoftwareSystems.CatalogApi.Containers.Infrastructure.Components.CatalogContext.Interfaces.CatalogItemsToListAsync")
                                    	.Use("dotnet.eShop.Architecture.SoftwareSystems.CatalogApi.Containers.Api.Components.CatalogApi.Interfaces.ChangeUriPlaceholder")
                                    	.Return(@"TypedResults.Ok"),
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