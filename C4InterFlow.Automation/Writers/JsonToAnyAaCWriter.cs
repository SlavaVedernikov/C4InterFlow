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
            YamlMappingNode rootNode = null;

            string[] yamlFiles = Directory.GetFiles(path, "*.yaml", SearchOption.AllDirectories);

            foreach (string yamlFile in yamlFiles)
            {
                using (StreamReader reader = new StreamReader(yamlFile))
                {
                    var yamlStream = new YamlStream();
                    yamlStream.Load(reader);

                    if (rootNode == null)
                    {
                        rootNode = (YamlMappingNode)yamlStream.Documents[0].RootNode;
                    }
                    else
                    {
                        MergeNodes(rootNode, (YamlMappingNode)yamlStream.Documents[0].RootNode);
                    }
                }
            }

            var document = new YamlDocument(rootNode);

            return GetJObject(document);
        }


        private JObject GetJObject(YamlDocument yamlDocument)
        {
            var yamlStream = new YamlStream(yamlDocument);
            var stringBuilder = new StringBuilder();

            using (var stringWriter = new StringWriter(stringBuilder))
            {
                // Pass 'false' to avoid getting random strings in the output
                yamlStream.Save(stringWriter, false);
            }

            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize(new StringReader(stringBuilder.ToString()));

            var serializer = new SerializerBuilder().JsonCompatible().Build();
            string jsonString = serializer.Serialize(yamlObject);

            return JObject.Parse(jsonString);
        }

        private void MergeNodes(YamlMappingNode target, YamlMappingNode source)
        {
            foreach (var sourceEntry in source.Children)
            {
                if (!target.Children.ContainsKey(sourceEntry.Key))
                {
                    target.Children.Add(sourceEntry.Key, sourceEntry.Value);
                }
                else
                {
                    if (sourceEntry.Value is YamlMappingNode sourceValueMapping && target.Children[sourceEntry.Key] is YamlMappingNode targetValueMapping)
                    {
                        MergeNodes(targetValueMapping, sourceValueMapping);
                    }
                    else
                    {
                        target.Children[sourceEntry.Key] = sourceEntry.Value;
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

        public virtual IAaCWriter AddSoftwareSystem(string name, string? boundary = null, string? label = null)
        {
            return this;
        }

        public virtual IAaCWriter AddSoftwareSystemInterface(
            string softwareSystemName,
            string name,
            string? label = null,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null)
        {
            return this;
        }

        public virtual IAaCWriter AddContainer(string softwareSystemName, string name, string? containerType = null, string? label = null)
        {
            return this;
        }

        public virtual IAaCWriter AddContainerInterface(
            string softwareSystemName,
            string containerName,
            string name,
            string? label = null,
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
