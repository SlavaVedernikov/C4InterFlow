using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using C4InterFlow.Structures;
using C4InterFlow.Automation.Readers;
using System.Text;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace C4InterFlow.Automation.Writers
{
    public abstract class JsonToAnyAaCWriter : IAaCWriter
    {
        protected string? ArchitectureInputPath { get; private set; }

        private JObject? RootJObject { get; set; }

        private Lazy<JObjectStructuresResolver> _elementsResolver;
        private JObjectStructuresResolver ElementsResolver
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
        public string ArchitectureNamespace { get; protected set; }

        public Dictionary<string, JObject> SoftwareSystemInterfaceAaCPathToJObjectMap { get; private set; } = new Dictionary<string, JObject>();
        public Dictionary<string, JObject> ContainerInterfaceAaCPathToJObjectMap { get; private set; } = new Dictionary<string, JObject>();

        protected JsonToAnyAaCWriter(string architectureInputPath)
        {
            ArchitectureInputPath = architectureInputPath;

            RootJObject = GetJsonObjectFromFiles(architectureInputPath);
        }

        public IEnumerable<SoftwareSystem> WithSoftwareSystems()
        {
            return RootJObject?.SelectTokens($"{ArchitectureNamespace}.SoftwareSystems.*")
                .Select(x => ElementsResolver.GetInstance<SoftwareSystem>(x.Path)).Where(x => x != null)!;
        }

        public IEnumerable<Interface> WithInterfaces(Structure owner)
        {
            return RootJObject?.SelectTokens($"{owner.Alias}.Interfaces.*")
                .Select(x => ElementsResolver.GetInstance<Interface>(x.Path)).Where(x => x != null)!;
        }

        public IEnumerable<Container> WithContainers(Structure owner)
        {
            return RootJObject?.SelectTokens($"{owner.Alias}.Containers.*")
                .Select(x => ElementsResolver.GetInstance<Container>(x.Path)).Where(x => x != null)!;
        }

        public JObject? GetSoftwareSystemInterface(string softwareSystemName, string name)
        {
            return RootJObject.SelectToken($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}.Interfaces.{name}") as JObject;
        }

        public JObject? GetContainerInterface(string softwareSystemName, string containerName, string name)
        {
            return RootJObject.SelectToken($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers.{containerName}.Interfaces.{name}") as JObject;
        }

        private JObject GetJsonObjectFromFiles(string path)
        {
            JObject rootNode = null;


            string[] jsonFiles = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);

            foreach (string jsonFile in jsonFiles)
            {
                using (StreamReader reader = new StreamReader(jsonFile))
                {
                    var json = reader.ReadToEnd();
                    var jsonObject = JObject.Parse(json);

                    if (rootNode == null)
                    {
                        rootNode = jsonObject;
                    }
                    else
                    {
                        MergeNodes(rootNode, jsonObject);
                    }
                }
            }


            return rootNode;
        }

        private void MergeNodes(JObject target, JObject source)
        {
            foreach (var sourceProperty in source.Properties())
            {
                JProperty targetProperty = target.Property(sourceProperty.Name);

                if (targetProperty == null)
                {
                    target.Add(sourceProperty);
                }
                else
                {
                    if (sourceProperty.Value is JObject sourceValueObject && targetProperty.Value is JObject targetValueObject)
                    {
                        MergeNodes(targetValueObject, sourceValueObject);
                    }
                    else
                    {
                        targetProperty.Value = sourceProperty.Value;
                    }
                }
            }
        }

        public virtual IAaCWriter AddActor(string name, string type, string? label = null)
        {
            return this;
        }

        public virtual IAaCWriter AddBusinessProcess(string name, string? label = null)
        {
            return this;
        }

        public virtual IAaCWriter AddSoftwareSystem(string name, string? boundary = null, string? label = null, string? description = null)
        {
            return this;
        }

        public virtual IAaCWriter AddSoftwareSystemInterface(
            string softwareSystemName,
            string name,
            string? label = null,
            string? description = null,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null)
        {
            return this;
        }

        public virtual IAaCWriter AddContainer(string softwareSystemName, string name, string? containerType = null, string? label = null, string? description = null, string? technology = null)
        {
            return this;
        }

        public virtual IAaCWriter AddContainerInterface(
            string softwareSystemName,
            string containerName,
            string name,
            string? label = null,
            string? description = null,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null)
        {
            return this;
        }

        public virtual IAaCWriter AddComponent(string softwareSystemName, string containerName, string name, ComponentType componentType = ComponentType.None)
        {
            return this;
        }

        public virtual IAaCWriter AddComponentInterface(
            string softwareSystemName,
            string containerName,
            string componentName,
            string name,
            string? label = null,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null)
        {
            return this;
        }

        public virtual string? GetComponentInterfaceAlias(string filePathPattern)
        {
            return string.Empty;
        }

        public virtual string GetFileExtension()
        {
            return string.Empty;
        }
    }
}
