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
            public partial class Api
            {
                public partial class Components
                {
                    public partial class CatalogApi
                    {
                        public partial class Interfaces
                        {
                            public partial class DeleteItemById : IInterfaceInstance
                            {
                                public static Interface Instance => new Interface(Utils.GetStructureAlias<DeleteItemById>(), "Delete Item By Id")
                                {
                                    Description = "",
                                    Path = "",
                                    IsPrivate = false,
                                    Protocol = "",
                                    Flow = new Flow(Utils.GetStructureAlias<DeleteItemById>())
                                    	.Use("DotNetEShop.SoftwareSystems.CatalogApi.Containers.Infrastructure.Components.CatalogContext.Interfaces.CatalogItemsSingleOrDefault")
                                    	.If(@"item is null")
                                    		.Return(@"TypedResults.NotFound")
                                    	.EndIf()
                                    	.Use("DotNetEShop.SoftwareSystems.CatalogApi.Containers.Infrastructure.Components.CatalogContext.Interfaces.CatalogItemsRemove")
                                    	.Use("DotNetEShop.SoftwareSystems.CatalogApi.Containers.Infrastructure.Components.CatalogContext.Interfaces.SaveChangesAsync")
                                    	.Return(@"TypedResults.NoContent"),
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