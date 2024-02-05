using C4InterFlow.Elements.Interfaces;
using Newtonsoft.Json.Linq;

namespace C4InterFlow.Elements
{
    public class JObjectElementsResolver : IElementsResolver
    {
        private JObject? RootJObject { get; init; }
        private string ArchitectureNamespace { get; init; }

        public JObjectElementsResolver()
        {
            RootJObject = null;
        }
        public JObjectElementsResolver(JObject rootJObject)
        {
            RootJObject = rootJObject;
            ArchitectureNamespace = GetArchitectureNamespace();
        }

        public T? GetInstance<T>(string? alias) where T : Structure
        {
            var result = default(T);

            if (string.IsNullOrEmpty(alias)) return result;

            var token = RootJObject?.SelectToken(alias);

            if(token == null) return result;

            var collectionToken = token?.Parent;
            var ownerToken = collectionToken?.Parent;

            if (collectionToken == null || ownerToken == null) return result;

            var label = Utils.GetLabel(alias.Split('.').Last()) ?? string.Empty;

            switch (collectionToken.Path.Split('.').Last()) {
                case "Interfaces":
                    result = new Interface(ownerToken.Path, alias, label)
                    {
                        Flow = token?["Flow"]?.ToObject<Flow>() ?? new Flow()
                    } as T;
                    break;
                case "SoftwareSystems":
                    result = new SoftwareSystem(alias, label) as T;
                    break;
                case "Containers":
                    result = new Container(ownerToken.Path, alias, label) as T;
                    break;
                case "Components":
                    result = new Component(ownerToken.Path, alias, label) as T;
                    break;
                case "Entities":
                    result = new Entity(ownerToken.Path, alias, label, EntityType.None) as T;
                    break;
                default:
                    break;
            }

            return result;
        }

        public IEnumerable<string> ResolveStructures(IEnumerable<string> structures)
        {
            var result = new List<string>();

            foreach (var item in structures)
            {
                result.AddRange(RootJObject.SelectTokens(item).Select(x => x.Path));
            }

            return result;
        }

        public IEnumerable<Interface> GetAllInterfaces()
        {
            var result = new List<Interface>();

            result.AddRange(GetInterfaces(RootJObject.SelectTokens($"{ArchitectureNamespace}.SoftwareSystems.*.Interfaces.*")));
            result.AddRange(GetInterfaces(RootJObject.SelectTokens($"{ArchitectureNamespace}.SoftwareSystems.*.Containers.*.Interfaces.*")));
            result.AddRange(GetInterfaces(RootJObject.SelectTokens($"{ArchitectureNamespace}.SoftwareSystems.*.Containers.*.Components.*.Interfaces.*")));

            return result;
        }

        private IEnumerable<Interface> GetInterfaces(IEnumerable<JToken> tokens)
        {
            var result = new List<Interface>();

            foreach (var token in tokens)
            {
                var interfaceInstance = GetInstance<Interface>(token.Path);
                if (interfaceInstance != null)
                {
                    result.Add(interfaceInstance);
                }
            }
            return result;
        }

        public IEnumerable<T> GetNestedInstances<T>(string? alias) where T : Structure
        {
            var result = new List<T>();

            if (string.IsNullOrEmpty(alias)) return result;

            var token = RootJObject?.SelectToken(alias);

            if (token == null) return result;

            foreach(var item in token.Children())
            {
                var instance = GetInstance<T>(item.Path);

                if(instance != null)
                {
                    result.Add(instance);
                }
            }

            return result;
        }

        private string GetArchitectureNamespace()
        {
            var result = string.Empty;
            var maxDepth = 10;
            var currentDepth = 1;

            while (string.IsNullOrEmpty(result) && currentDepth <= maxDepth)
            {
                var token = RootJObject.SelectToken($"{string.Concat(Enumerable.Repeat("*.", currentDepth))}SoftwareSystems");

                if(token != null )
                {
                    result = token.Parent.Path;
                }
                currentDepth++;
            }

            return result;
        }
    }
}
