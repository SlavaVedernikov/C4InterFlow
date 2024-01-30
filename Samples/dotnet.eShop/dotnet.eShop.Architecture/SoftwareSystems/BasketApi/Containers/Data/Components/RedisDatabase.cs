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
            public partial class Data
            {
                public partial class Components
                {
                    public partial class RedisDatabase : IComponentInstance
                    {
                        public const string ALIAS = "dotnet.eShop.Architecture.SoftwareSystems.BasketApi.Containers.Data.Components.RedisDatabase";
                        public static Component Instance => new Component(dotnet.eShop.Architecture.SoftwareSystems.BasketApi.Containers.Data.ALIAS, ALIAS, "Redis Database")
                        {
                            ComponentType = ComponentType.None,
                            Description = "",
                            Technology = ""
                        };

                        public partial class Interfaces
                        {
                        }
                    }
                }
            }
        }
    }
}