using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;
using Newtonsoft.Json;
using System.Dynamic;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace C4InterFlow.Automation.Writers
{
    public class CsvToJsonAaCWriter : CsvToAnyAaCWriter
    {
        public JObject JsonArchitectureAsCode { get; private set; }
        public string ArchitectureOutputPath { get; private set; }
        protected CsvToJsonAaCWriter(string architectureInputPath):base(architectureInputPath)
        {
        }

        public static CsvToJsonAaCWriter WithCsvData(string csvRootPath)
        {
            return new CsvToJsonAaCWriter(csvRootPath);
        }
        public CsvToJsonAaCWriter WithArchitectureRootNamespace(string architectureRootNamespace)
        {
            ArchitectureNamespace = architectureRootNamespace.Trim();

            var architectureNamespaceSegments = ArchitectureNamespace.Split('.');
            var currentObject = JsonArchitectureAsCode = new JObject();
            foreach (var segment in architectureNamespaceSegments)
            {
                var segmentObject = new JObject();
                currentObject.Add(segment, segmentObject);
                currentObject = segmentObject;
            }

            return this;
        }

        public CsvToJsonAaCWriter WithSoftwareSystemsCollection()
        {
            var architectureNamespaceRoot = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}") as JObject;
            if (architectureNamespaceRoot != null && !architectureNamespaceRoot.ContainsKey("SoftwareSystems"))
            {
                architectureNamespaceRoot.Add("SoftwareSystems", new JObject());
            }

            return this;
        }

        public CsvToJsonAaCWriter WithActorsCollection()
        {
            var architectureNamespaceRoot = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}") as JObject;
            if (architectureNamespaceRoot != null && !architectureNamespaceRoot.ContainsKey("Actors"))
            {
                architectureNamespaceRoot.Add("Actors", new JObject());
            }

            return this;
        }

        public CsvToJsonAaCWriter WithBusinessProcessesCollection()
        {
            var architectureNamespaceRoot = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}") as JObject;
            if (architectureNamespaceRoot != null && !architectureNamespaceRoot.ContainsKey("BusinessProcesses"))
            {
                architectureNamespaceRoot.Add("BusinessProcesses", new JObject());
            }

            return this;
        }

        public CsvToJsonAaCWriter WithArchitectureOutputPath(string architectureOutputPath)
        {
            ArchitectureOutputPath = architectureOutputPath;

            var directoryPath = Path.GetDirectoryName(ArchitectureOutputPath);

            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return this;
        }

        public void WriteArchitecture()
        {
            var json = JsonConvert.SerializeObject(JsonArchitectureAsCode, Formatting.Indented);
            File.WriteAllText(ArchitectureOutputPath, json);

            var jsonObject = JsonConvert.DeserializeObject<ExpandoObject>(json);
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(jsonObject);
            File.WriteAllText(ArchitectureOutputPath.Replace(".json", ".yaml"), yaml);

            JsonArchitectureAsCode = new JObject();
        }

        public IEnumerable<CsvDataProvider.SoftwareSystem> WithSoftwareSystems()
        {
            return DataProvider.SoftwareSystemRecords.Where(x => !string.IsNullOrEmpty(x.Alias.Trim()));
        }

        public IEnumerable<CsvDataProvider.Actor> WithActors()
        {
            return DataProvider.ActorRecords.Where(x => !string.IsNullOrEmpty(x.Alias.Trim()));
        }

        public IEnumerable<CsvDataProvider.BusinessProcess> WithBusinessProcesses()
        {
            return DataProvider.BusinessProcessRecords.Where(x => !string.IsNullOrEmpty(x.Alias.Trim()));
        }

        public override CsvToJsonAaCWriter AddActor(string name, string type, string? label = null)
        {
            var actorsObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.Actors") as JObject;

            if (actorsObject != null)
            {
                actorsObject.Add(
                    name,
                    new JObject
                    {
                        { "Type", type },
                        { "Label", string.IsNullOrEmpty(label) ? AnyCodeWriter.GetLabel(name) : label },
                    });
            }

            return this;
        }

        public override CsvToJsonAaCWriter AddBusinessProcess(string name, string? label = null)
        {
            var businessProcess = DataProvider.BusinessProcessRecords.FirstOrDefault(x => x.Alias == name);

            if (businessProcess == null) return this;

            var businessProcessesObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.BusinessProcesses") as JObject;

            if (businessProcessesObject != null)
            {
                var businessActivitiesJArray = new JArray();
                foreach (var businessActivity in GetBusinessProcessActivities(businessProcess))
                {
                    var actor = businessActivity.Actor;
                    businessActivitiesJArray.Add(new JObject()
                    {
                        { "Label", businessActivity.Label },
                        { "Actor", actor },
                        {
                            "Flow", new JObject(businessActivity.Flow)
                        }
                    });
                }

                businessProcessesObject.Add(
                    name,
                    new JObject
                    {
                        { "Label", string.IsNullOrEmpty(label) ? AnyCodeWriter.GetLabel(name) : label },
                        { "Activities", businessActivitiesJArray }
                    });
            }

            return this;
        }

        public override CsvToJsonAaCWriter AddSoftwareSystem(string name, string? boundary = null, string label = null)
        {
            var softwareSystemsObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems") as JObject;

            if (softwareSystemsObject != null)
            {
                var softwareSystemObject = new JObject
                    {
                        { "Boundary", boundary != null ? boundary : "Internal" },
                        { "Containers", new JObject() },
                        { "Interfaces", new JObject() }
                    };

                AddLabel(softwareSystemObject, name, label);

                softwareSystemsObject.Add(name, softwareSystemObject);
            }
            return this;
        }
        public override CsvToJsonAaCWriter AddSoftwareSystemInterface(
            string softwareSystemName,
            string name,
            string? label = null,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null)
        {
            var softwareSystemInterfacesObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}.Interfaces") as JObject;

            if (softwareSystemInterfacesObject != null)
            {
                var softwareSystemInterfaceObject = new JObject();

                AddLabel(softwareSystemInterfaceObject, name, label);

                softwareSystemInterfacesObject.Add(name, softwareSystemInterfaceObject);

                if (!SoftwareSystemInterfaceAaCPathToCsvRecordMap.Keys.Contains(softwareSystemInterfaceObject.Path))
                {
                    var softwareSystemInterface = DataProvider.SoftwareSystemInterfaceRecords.FirstOrDefault(x => x.Name == name);
                    if (softwareSystemInterface != null)
                    {
                        SoftwareSystemInterfaceAaCPathToCsvRecordMap.Add(softwareSystemInterfaceObject.Path, softwareSystemInterface);
                    }
                }
            }

            return this;
        }

        public override CsvToJsonAaCWriter AddContainer(string softwareSystemName, string name, string? containerType = null, string? label = null)
        {
            var containersObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers") as JObject;

            if (containersObject != null)
            {
                var containerObject = new JObject
                    {
                        { "ContainerType", containerType != null ? containerType : "None" },
                        { "Components", new JObject() },
                        { "Interfaces", new JObject() }
                    };

                containersObject.Add(name, containerObject);

                AddLabel(containerObject, name, label);
            }

            return this;
        }

        public override CsvToJsonAaCWriter AddContainerInterface(
            string softwareSystemName,
            string containerName,
            string name,
            string? label = null,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null)
        {
            var containerInterfacesObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers.{containerName}.Interfaces") as JObject;

            if (containerInterfacesObject != null)
            {
                var containerInterfaceObject = new JObject();

                AddLabel(containerInterfaceObject, name, label);

                containerInterfacesObject.Add(name, containerInterfaceObject);

                if (!ContainerInterfaceAaCPathToCsvRecordMap.Keys.Contains(containerInterfaceObject.Path))
                {
                    var containerInterface = DataProvider.ContainerInterfaceRecords.FirstOrDefault(x => x.Name == name);
                    if (containerInterface != null)
                    {
                        ContainerInterfaceAaCPathToCsvRecordMap.Add(containerInterfaceObject.Path, containerInterface);
                    }
                }
            }

            return this;
        }

        public IEnumerable<JObject> WithSoftwareSystemInterfaceObjects(string softwareSystemName)
        {
            var result = JsonArchitectureAsCode.SelectTokens($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}.Interfaces.*").Select(x => x as JObject);

            return result;
        }

        public IEnumerable<JObject> WithContainerInterfaceObjects(string? softwareSystemName = null, string? containerName = null)
        {
            var result = JsonArchitectureAsCode.SelectTokens($"{ArchitectureNamespace}.SoftwareSystems.{(softwareSystemName != null ? softwareSystemName : "*")}.Containers.{(containerName != null ? containerName : "*")}.Interfaces.*").Select(x => x as JObject);

            return result;
        }

        private void AddLabel(JObject jObject, string name, string? label)
        {
            if (!string.IsNullOrEmpty(label))
            {
                var inferredLabel = Utils.GetLabel(name);

                if (!label.Equals(inferredLabel))
                {
                    jObject.Add("Label", label);
                }
            }
        }
    }
}
