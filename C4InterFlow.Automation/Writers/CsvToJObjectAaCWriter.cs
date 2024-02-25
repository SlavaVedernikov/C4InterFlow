using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;
using Newtonsoft.Json;
using System.Dynamic;
using System.Reflection.Emit;
using System.Xml.Linq;
using System.IO;
using Newtonsoft.Json.Converters;

namespace C4InterFlow.Automation.Writers
{
    public abstract class CsvToJObjectAaCWriter : CsvToAnyAaCWriter
    {
        public JObject JsonArchitectureAsCode { get; protected set; }
        public string ArchitectureOutputPath { get; private set; }
        protected CsvToJObjectAaCWriter(string architectureInputPath):base(architectureInputPath)
        {
        }

        public CsvToJObjectAaCWriter WithArchitectureRootNamespace(string architectureRootNamespace)
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

        public CsvToJObjectAaCWriter WithSoftwareSystemsCollection()
        {
            var architectureNamespaceRoot = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}") as JObject;
            if (architectureNamespaceRoot != null && !architectureNamespaceRoot.ContainsKey("SoftwareSystems"))
            {
                architectureNamespaceRoot.Add("SoftwareSystems", new JObject());
            }

            return this;
        }

        public CsvToJObjectAaCWriter WithActorsCollection()
        {
            var architectureNamespaceRoot = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}") as JObject;
            if (architectureNamespaceRoot != null && !architectureNamespaceRoot.ContainsKey("Actors"))
            {
                architectureNamespaceRoot.Add("Actors", new JObject());
            }

            return this;
        }

        public CsvToJObjectAaCWriter WithBusinessProcessesCollection()
        {
            var architectureNamespaceRoot = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}") as JObject;
            if (architectureNamespaceRoot != null && !architectureNamespaceRoot.ContainsKey("BusinessProcesses"))
            {
                architectureNamespaceRoot.Add("BusinessProcesses", new JObject());
            }

            return this;
        }

        public abstract void WriteArchitecture(string architectureOutputPath, string fileName);


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

        public override CsvToJObjectAaCWriter AddActor(string name, string type, string? label = null)
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

        public override CsvToJObjectAaCWriter AddBusinessProcess(string name, string? label = null)
        {
            var businessProcess = DataProvider.BusinessProcessRecords.FirstOrDefault(x => x.Alias == name);

            if (businessProcess == null) return this;

            var businessProcessesObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.BusinessProcesses") as JObject;

            if (businessProcessesObject != null)
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new StringEnumConverter());

                var businessActivitiesJArray = new JArray();
                foreach (var businessActivity in GetBusinessProcessActivities(businessProcess))
                {
                    var actor = businessActivity.Actor;
                    businessActivitiesJArray.Add(new JObject()
                    {
                        { "Label", businessActivity.Label },
                        { "Actor", actor },
                        {
                            "Flow", JObject.FromObject(businessActivity.Flow, serializer)
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

        public override CsvToJObjectAaCWriter AddSoftwareSystem(string name, string? boundary = null, string? label = null)
        {
            var softwareSystemsObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems") as JObject;

            if (softwareSystemsObject != null)
            {
                var softwareSystemObject = new JObject
                    {
                        { "Label", GetLabel(name, label) },
                        { "Boundary", boundary != null ? boundary : "Internal" }
                    };

                softwareSystemsObject.Add(name, softwareSystemObject);
            }
            return this;
        }
        public override CsvToJObjectAaCWriter AddSoftwareSystemInterface(
            string softwareSystemName,
            string name,
            string? label = null,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null)
        {
            var softwareSystemInterfacesObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}.Interfaces") as JObject;

            if (softwareSystemInterfacesObject == null)
            {
                var softwareSystemObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}") as JObject;
                if (softwareSystemObject != null)
                {
                    softwareSystemObject.Add("Interfaces", new JObject());
                }

                softwareSystemInterfacesObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers") as JObject;
            }

            if (softwareSystemInterfacesObject != null)
            {
                var softwareSystemInterfaceObject = new JObject()
                {
                    { "Label", GetLabel(name, label) }
                };

                softwareSystemInterfacesObject.Add(name, softwareSystemInterfaceObject);

                if (!SoftwareSystemInterfaceAaCPathToCsvRecordMap.Keys.Contains(softwareSystemInterfaceObject.Path))
                {
                    var softwareSystemInterface = DataProvider.SoftwareSystemInterfaceRecords.FirstOrDefault(x => x.Alias == softwareSystemInterfaceObject.Path.Replace($"{ArchitectureNamespace}.SoftwareSystems.", string.Empty));
                    if (softwareSystemInterface != null)
                    {
                        SoftwareSystemInterfaceAaCPathToCsvRecordMap.Add(softwareSystemInterfaceObject.Path, softwareSystemInterface);
                    }
                }
            }

            return this;
        }

        public override CsvToJObjectAaCWriter AddContainer(string softwareSystemName, string name, string? containerType = null, string? label = null)
        {
            var containersObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers") as JObject;
            if (containersObject == null)
            {
                var softwareSystemObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}") as JObject;
                if (softwareSystemObject != null)
                {
                    softwareSystemObject.Add("Containers", new JObject());
                }

                containersObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers") as JObject;
            }

            if (containersObject != null)
            {
                var containerObject = new JObject
                    {
                        { "Label", GetLabel(name, label) },
                        { "ContainerType", containerType != null ? containerType : "None" },
                    };

                containersObject.Add(name, containerObject);
            }

            return this;
        }

        public override CsvToJObjectAaCWriter AddContainerInterface(
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

            if (containerInterfacesObject == null)
            {
                var containerObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers.{containerName}") as JObject;
                if (containerObject != null)
                {
                    containerObject.Add("Interfaces", new JObject());
                }

                containerInterfacesObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers.{containerName}.Interfaces") as JObject;
            }

            if (containerInterfacesObject != null)
            {
                var containerInterfaceObject = new JObject()
                {
                    { "Label", GetLabel(name, label) }
                };

                containerInterfacesObject.Add(name, containerInterfaceObject);

                if (!ContainerInterfaceAaCPathToCsvRecordMap.Keys.Contains(containerInterfaceObject.Path))
                {
                    var containerInterface = DataProvider.ContainerInterfaceRecords.FirstOrDefault(x => x.Alias == containerInterfaceObject.Path.Replace($"{ArchitectureNamespace}.SoftwareSystems.", string.Empty));
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

        private string GetLabel(string name, string? label)
        {
            var result = label;
            if (string.IsNullOrEmpty(result))
            {
                result = Utils.GetLabel(name);
            }

            return result ?? string.Empty;
        }
    }
}
