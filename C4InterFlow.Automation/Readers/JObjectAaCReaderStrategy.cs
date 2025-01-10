using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Reflection;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace C4InterFlow.Automation.Readers
{
    public abstract class JObjectAaCReaderStrategy : AaCReaderStrategy<JObjectStructuresResolver>
    {
        private JObject? RootJObject { get; set; }
        public JObjectAaCReaderStrategy()
        {

        }
        public JObjectAaCReaderStrategy(JObject rootJObject)
        {
            RootJObject = rootJObject;
        }

        public override void Initialise(string[]? architectureInputPaths, string[]? viewsInputPaths, Dictionary<string, string>? parameters)
        {
            RootJObject = GetJsonObjectFromFiles(architectureInputPaths, viewsInputPaths);

            RootJObject = ExtendJsonObject(RootJObject);

            base.Initialise(architectureInputPaths, viewsInputPaths,parameters);
        }

        private Lazy<JObjectStructuresResolver> _elementsResolver;
        public override JObjectStructuresResolver ElementsResolver
        {
            get
            {
                if (_elementsResolver == null)
                {
                    _elementsResolver = new Lazy<JObjectStructuresResolver>(() => new JObjectStructuresResolver(RootJObject ?? new JObject()));
                }
                return _elementsResolver.Value;
            }
        }

        public override string GetComponentInterfaceAlias()
        {
            var result = string.Empty;

            var selectedToken = RootJObject.SelectToken("..SoftwareSystems.*.Containers.*.Components.*.Interfaces.*");

            if (selectedToken != null)
            {
                result = selectedToken.Path;
            }

            return result;
        }

        public override string GetComponentInterfaceAlias(string filePath)
        {
            var result = string.Empty;

            var jsonObject = GetJsonObjectFromFile(filePath);
            var selectedToken = jsonObject.SelectToken("..SoftwareSystems.*.Containers.*.Components.*.Interfaces.*");

            if (selectedToken != null)
            {
                result = selectedToken.Path;
            }

            return result;
        }

        private JObject ExtendJsonObject(JObject jObject)
        {
            if (jObject != null)
            {
                jObject = AddTypeInstance(jObject, typeof(SoftwareSystems.ExternalSystem));
                jObject = AddTypeInstance(jObject, typeof(SoftwareSystems.ExternalSystem.Interfaces.ExternalInterface));
            }

            return jObject;
        }

        private JObject AddTypeInstance(JObject jObject, Type type)
        {
            if (jObject != null)
            {
                var namespaceSegments = type.FullName.Split('+').First().Split('.');
                var typeSegments = type.FullName.Split('+').Skip(1);
                var path = string.Empty;

                foreach (var typeNamespaceSegment in namespaceSegments)
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        if (!jObject.ContainsKey(typeNamespaceSegment))
                        {
                            jObject.Add(typeNamespaceSegment, new JObject());
                        }

                        path = typeNamespaceSegment;
                    }
                    else
                    {
                        var currentJObject = jObject.SelectToken(path) as JObject;

                        if (currentJObject != null && !currentJObject.ContainsKey(typeNamespaceSegment))
                        {
                            currentJObject.Add(typeNamespaceSegment, new JObject());
                        }

                        path = $"{path}.{typeNamespaceSegment}";
                    }
                }

                foreach (var typeSegment in typeSegments)
                {
                    var currentJObject = jObject.SelectToken(path) as JObject;

                    if (currentJObject != null && !currentJObject.ContainsKey(typeSegment))
                    {
                        currentJObject.Add(typeSegment, new JObject());
                    }

                    path = $"{path}.{typeSegment}";
                }

                var instanceObject = Activator.CreateInstance(type);
                var propertyInfo = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Instance);

                var instance = propertyInfo?.GetValue(instanceObject);

                var lastJObject = jObject.SelectToken(path) as JObject;
                lastJObject.Parent.Parent[path.Split('.').Last()] = JObject.FromObject(instance);


            }

            return jObject;
        }

        protected abstract JObject GetJsonObjectFromFile(string filePath);
        protected abstract JObject GetJsonObjectFromFiles(string[] aacPaths, string[] viewsPaths);
    }


}
