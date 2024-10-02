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
                            public partial class UpdateItem : IInterfaceInstance
                            {
                                public static Interface Instance => new Interface(Utils.GetStructureAlias<UpdateItem>(), "Update Item")
                                {
                                    Description = "",
                                    Path = "",
                                    IsPrivate = false,
                                    Protocol = "",
                                    Flow = new Flow(Utils.GetStructureAlias<UpdateItem>())
                                    	.If(@"catalogItem == null")
                                    		.Return(@"TypedResults.NotFound")
                                    	.EndIf()
                                    	.If(@"priceEntry.IsModified")
                                    		.Else()
                                    		.Use("DotNetEShop.SoftwareSystems.CatalogApi.Containers.Infrastructure.Components.CatalogContext.Interfaces.SaveChangesAsync")
                                    	.EndIf()
                                    	.Return(@"TypedResults.Created"),
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