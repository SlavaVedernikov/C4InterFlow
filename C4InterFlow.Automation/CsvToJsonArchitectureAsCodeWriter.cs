using Microsoft.CodeAnalysis;
using CsvHelper;
using System.Globalization;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;
using Newtonsoft.Json;
using System.Dynamic;
using C4InterFlow.Elements;

namespace C4InterFlow.Automation
{
    public class CsvToJsonArchitectureAsCodeWriter : CsvToAnyArchitectureAsCodeWriter
    {
        public JObject JsonArchitectureAsCode { get; private set; }
        public string ArchitectureOutputPath { get; private set; }
        protected CsvToJsonArchitectureAsCodeWriter(string architectureInputPath)
        {
            LoadData(architectureInputPath);
        }

        public static CsvToJsonArchitectureAsCodeWriter WithCsvData(string csvRootPath)
        {
            return new CsvToJsonArchitectureAsCodeWriter(csvRootPath);
        }
        public CsvToJsonArchitectureAsCodeWriter WithArchitectureRootNamespace(string architectureRootNamespace)
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

            currentObject.Add("SoftwareSystems", new JObject());

            return this;
        }

        public CsvToJsonArchitectureAsCodeWriter WithArchitectureOutputPath(string architectureOutputPath)
        {
            ArchitectureOutputPath = architectureOutputPath;
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
        }

        public IEnumerable<SoftwareSystem> WithSoftwareSystems()
        {
            return SoftwareSystemRecords.Where(x => !string.IsNullOrEmpty(x.Alias.Trim()));
        }

        public CsvToJsonArchitectureAsCodeWriter AddSoftwareSystemObject(string softwareSystemName, string? boundary = null)
        {
            var softwareSystemsObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems") as JObject;

            if(softwareSystemsObject != null )
            {
                softwareSystemsObject.Add(
                    softwareSystemName, 
                    new JObject
                    {
                        { "Boundary", boundary != null ? boundary : "Internal" },
                        { "Containers", new JObject() },
                        { "Interfaces", new JObject() }
                    }
                );
            }
            return this;
        }
        public CsvToJsonArchitectureAsCodeWriter AddSoftwareSystemInterfaceObject(SoftwareSystemInterface softwareSystemInterface)
        {
            var softwareSystemName = softwareSystemInterface.SoftwareSystemAlias;
            var interfaceName = softwareSystemInterface.Alias.Split('.').Last();
            var softwareSystemInterfacesObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}.Interfaces") as JObject;

            if (softwareSystemInterfacesObject != null)
            {
                var softwareSystemInterfaceObject = new JObject();

                softwareSystemInterfacesObject.Add(
                    interfaceName, softwareSystemInterfaceObject);

                if (!SoftwareSystemInterfaceClassFileNameMap.Keys.Contains(softwareSystemInterfaceObject.Path))
                {
                    SoftwareSystemInterfaceClassFileNameMap.Add(softwareSystemInterfaceObject.Path, softwareSystemInterface);
                }
            }

            return this;
        }

        public CsvToJsonArchitectureAsCodeWriter AddContainerObject(string softwareSystemName, string containerName, string? containerType = null )
        {
            var softwareSystemContainersObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers") as JObject;

            if (softwareSystemContainersObject != null)
            {
                softwareSystemContainersObject.Add(
                    containerName,
                    new JObject
                    {
                        { "ContainerType", containerType != null ? containerType : "None" },
                        { "Components", new JObject() },
                        { "Interfaces", new JObject() }
                    }
                );
            }

            return this;
        }

        public CsvToJsonArchitectureAsCodeWriter AddContainerInterfaceObject(ContainerInterface containerInterface)
        {
            var containerAliasSegments = containerInterface.ContainerAlias.Split('.');
            var softwareSystemName = containerAliasSegments[Array.IndexOf(containerAliasSegments, "Containers") - 1];
            var containerName = containerAliasSegments.Last();
            var interfaceName = containerInterface.Alias.Split('.').Last();

            var containerInterfacesObject = JsonArchitectureAsCode.SelectToken($"{ArchitectureNamespace}.SoftwareSystems.{softwareSystemName}.Containers.{containerName}.Interfaces") as JObject;

            if (containerInterfacesObject != null)
            {
                var containerInterfaceObject = new JObject();
                containerInterfacesObject.Add(
                    interfaceName, containerInterfaceObject);

                if (!ContainerInterfaceClassFileNameMap.Keys.Contains(containerInterfaceObject.Path))
                {
                    ContainerInterfaceClassFileNameMap.Add(containerInterfaceObject.Path, containerInterface);
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
    }
}
