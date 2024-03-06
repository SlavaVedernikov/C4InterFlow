using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Collections.Generic;
using System.Globalization;

namespace C4InterFlow.Automation
{
    public class CsvDataProvider
    {
        public enum Mode
        {
            None,
            Read,
            Write
        }
        protected const string FILE_SOFTWARE_SYSTEMS = "Software Systems";
        protected const string FILE_SOFTWARE_SYSTEM_ATTRIBUTES = "Software System Attributes";
        protected const string FILE_SOFTWARE_SYSTEM_INTERFACES = "Software System Interfaces";
        protected const string FILE_SOFTWARE_SYSTEM_INTERFACE_FLOWS = "Software System Interface Flows";
        protected const string FILE_CONTAINERS = "Containers";
        protected const string FILE_CONTAINER_ATTRIBUTES = "Container Attributes";
        protected const string FILE_CONTAINER_INTERFACES = "Container Interfaces";
        protected const string FILE_CONTAINER_INTERFACE_FLOWS = "Container Interface Flows";
        protected const string FILE_ACTORS = "Actors";
        protected const string FILE_ACTOR_TYPES = "Actor Types";
        protected const string FILE_BUSINESS_PROCESSES = "Business Processes";
        protected const string FILE_ACTIVITIES = "Activities";
        protected const string FILE_ATTRIBUTES = "Attributes";

        private string DataPath { get; set; }
        public CsvDataProvider(string dataPath, Mode mode = Mode.Read) {
            DataPath = dataPath;

            if (mode == Mode.Write)
            {
                ActorRecords = new List<Actor>();
                ActorTypeRecords = new List<ActorType>();
                SoftwareSystemRecords = new List<SoftwareSystem>();
                SoftwareSystemAttributeRecords = new List<SoftwareSystemAttribute>();
                SoftwareSystemInterfaceRecords = new List<SoftwareSystemInterface>();
                SoftwareSystemInterfaceFlowRecords = new List<SoftwareSystemInterfaceFlow>();
                ContainerRecords = new List<Container>();
                ContainerAttributeRecords = new List<ContainerAttribute>();
                ContainerInterfaceRecords = new List<ContainerInterface>();
                ContainerInterfaceFlowRecords = new List<ContainerInterfaceFlow>();
                BusinessProcessRecords = new List<BusinessProcess>();
                ActivityRecords = new List<Activity>();
                AttributeRecords = new List<Attribute>();
            }
            else
            {
                using (var reader = new StreamReader(@$"{DataPath}\{FILE_ACTORS}.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    ActorRecords = csv.GetRecords<Actor>().ToList();
                }

                using (var reader = new StreamReader(@$"{DataPath}\{FILE_ACTOR_TYPES}.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    ActorTypeRecords = csv.GetRecords<ActorType>().ToList();
                }

                using (var reader = new StreamReader(@$"{DataPath}\{FILE_SOFTWARE_SYSTEMS}.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    SoftwareSystemRecords = csv.GetRecords<SoftwareSystem>().ToList();
                }

                using (var reader = new StreamReader(@$"{DataPath}\{FILE_SOFTWARE_SYSTEM_ATTRIBUTES}.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    SoftwareSystemAttributeRecords = csv.GetRecords<SoftwareSystemAttribute>().ToList();
                }

                using (var reader = new StreamReader(@$"{DataPath}\{FILE_SOFTWARE_SYSTEM_INTERFACES}.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    SoftwareSystemInterfaceRecords = csv.GetRecords<SoftwareSystemInterface>().ToList();
                }

                using (var reader = new StreamReader(@$"{DataPath}\{FILE_SOFTWARE_SYSTEM_INTERFACE_FLOWS}.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    SoftwareSystemInterfaceFlowRecords = csv.GetRecords<SoftwareSystemInterfaceFlow>().ToList();
                }

                using (var reader = new StreamReader(@$"{DataPath}\{FILE_CONTAINERS}.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    ContainerRecords = csv.GetRecords<Container>().ToList();
                }

                using (var reader = new StreamReader(@$"{DataPath}\{FILE_CONTAINER_ATTRIBUTES}.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    ContainerAttributeRecords = csv.GetRecords<ContainerAttribute>().ToList();
                }

                using (var reader = new StreamReader(@$"{DataPath}\{FILE_CONTAINER_INTERFACES}.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    ContainerInterfaceRecords = csv.GetRecords<ContainerInterface>().ToList();
                }

                using (var reader = new StreamReader(@$"{DataPath}\{FILE_CONTAINER_INTERFACE_FLOWS}.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    ContainerInterfaceFlowRecords = csv.GetRecords<ContainerInterfaceFlow>().ToList();
                }

                using (var reader = new StreamReader(@$"{DataPath}\{FILE_ACTORS}.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    ActorRecords = csv.GetRecords<Actor>().ToList();
                }

                using (var reader = new StreamReader(@$"{DataPath}\{FILE_BUSINESS_PROCESSES}.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    BusinessProcessRecords = csv.GetRecords<BusinessProcess>().ToList();
                }

                using (var reader = new StreamReader(@$"{DataPath}\{FILE_ACTIVITIES}.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    ActivityRecords = csv.GetRecords<Activity>().ToList();
                }

                using (var reader = new StreamReader(@$"{DataPath}\{FILE_ATTRIBUTES}.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    AttributeRecords = csv.GetRecords<Attribute>().ToList();
                }
            }
        }
        public IList<Actor> ActorRecords { get; init; }
        public IList<ActorType> ActorTypeRecords { get; init; }
        public IList<SoftwareSystem> SoftwareSystemRecords { get; init; }
        public IList<SoftwareSystemAttribute> SoftwareSystemAttributeRecords { get; init; }
        public IList<SoftwareSystemInterface> SoftwareSystemInterfaceRecords { get; init; }
        public IList<SoftwareSystemInterfaceFlow> SoftwareSystemInterfaceFlowRecords { get; init; }
        public IList<Container> ContainerRecords { get; init; }
        public IList<ContainerAttribute> ContainerAttributeRecords { get; init; }
        public IList<ContainerInterface> ContainerInterfaceRecords { get; init; }
        public IList<ContainerInterfaceFlow> ContainerInterfaceFlowRecords { get; init; }
        public IList<BusinessProcess> BusinessProcessRecords { get; init; }
        public IList<Activity> ActivityRecords { get; init; }
        public IList<Attribute> AttributeRecords { get; init; }
        public void WriteData()
        {
            using (var writer = new StreamWriter(@$"{DataPath}\{FILE_ACTORS}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(ActorRecords);
            }

            using (var writer = new StreamWriter(@$"{DataPath}\{FILE_SOFTWARE_SYSTEMS}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(SoftwareSystemRecords);
            }

            using (var writer = new StreamWriter(@$"{DataPath}\{FILE_SOFTWARE_SYSTEM_ATTRIBUTES}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(SoftwareSystemAttributeRecords);
            }

            using (var writer = new StreamWriter(@$"{DataPath}\{FILE_SOFTWARE_SYSTEM_INTERFACES}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(SoftwareSystemInterfaceRecords);
            }

            using (var writer = new StreamWriter(@$"{DataPath}\{FILE_SOFTWARE_SYSTEM_INTERFACE_FLOWS}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(SoftwareSystemInterfaceFlowRecords);
            }

            using (var writer = new StreamWriter(@$"{DataPath}\{FILE_CONTAINERS}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(ContainerRecords);
            }

            using (var writer = new StreamWriter(@$"{DataPath}\{FILE_CONTAINER_ATTRIBUTES}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(ContainerAttributeRecords);
            }

            using (var writer = new StreamWriter(@$"{DataPath}\{FILE_CONTAINER_INTERFACES}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(ContainerInterfaceRecords);
            }

            using (var writer = new StreamWriter(@$"{DataPath}\{FILE_CONTAINER_INTERFACE_FLOWS}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(ContainerInterfaceFlowRecords);
            }

            using (var writer = new StreamWriter(@$"{DataPath}\{FILE_BUSINESS_PROCESSES}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(BusinessProcessRecords);
            }

            using (var writer = new StreamWriter(@$"{DataPath}\{FILE_ACTIVITIES}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(ActivityRecords);
            }

            using (var writer = new StreamWriter(@$"{DataPath}\{FILE_ATTRIBUTES}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(AttributeRecords);
            }
        }

        public record SoftwareSystem
        {
            [Name("Name")]
            [Index(1)]
            public string Name { get; set; }

            [Name("Is External")]
            [Index(2)]
            [BooleanTrueValues("Yes", "yes")]
            [BooleanFalseValues("No", "no", "")]
            public bool IsExternal { get; set; }

            [Name("Alias")]
            [Index(3)]
            public string Alias { get; set; }

            public string GetBoundary()
            {
                return IsExternal ? "External" : "Internal";
            }

            public IEnumerable<SoftwareSystemAttribute> WithAttributes(CsvDataProvider dataProvider)
            {
                return dataProvider.SoftwareSystemAttributeRecords.Where(x => !string.IsNullOrEmpty(x.SoftwareSystem.Trim()) &&
                    x.SoftwareSystem == Alias);
            }

            public IEnumerable<SoftwareSystemInterface> WithInterfaces(CsvDataProvider dataProvider)
            {
                return dataProvider.SoftwareSystemInterfaceRecords.Where(x => !string.IsNullOrEmpty(x.SoftwareSystem.Trim()) &&
                    x.SoftwareSystem == Alias);
            }

            public IEnumerable<Container> WithContainers(CsvDataProvider dataProvider)
            {
                return dataProvider.ContainerRecords.Where(x => !string.IsNullOrEmpty(x.SoftwareSystem.Trim()) &&
                    x.SoftwareSystem == Alias);
            }
        }

        public record SoftwareSystemAttribute
        {
            [Name("Software System")]
            [Index(1)]
            public string SoftwareSystem { get; set; }

            [Name("Attribute")]
            [Index(2)]
            public string Attribute { get; set; }

            [Name("Value")]
            [Index(3)]
            public string Value { get; set; }

            public bool TryGetAttributeName(CsvDataProvider dataProvider, out string? name)
            {
                name = dataProvider.AttributeRecords.FirstOrDefault(x => x.Alias == Attribute)?.Name;
                return name != null;
            }
        }

        public record SoftwareSystemInterface
        {
            [Name("Software System")]
            [Index(1)]
            public string SoftwareSystem { get; set; }

            [Name("Name")]
            [Index(2)]
            public string Name { get; set; }

            [Name("Alias")]
            [Index(3)]
            public string Alias { get; set; }

            public IEnumerable<SoftwareSystemInterfaceFlow> WithUses(CsvDataProvider dataProvider)
            {
                return dataProvider.SoftwareSystemInterfaceFlowRecords.Where(x => !string.IsNullOrEmpty(x.SoftwareSystemInterface.Trim()) &&
                    x.SoftwareSystemInterface == Alias);
            }
        }

        public record SoftwareSystemInterfaceFlow
        {
            [Name("Software System Interface")]
            [Index(1)]
            public string SoftwareSystemInterface { get; set; }

            [Name("Uses Software System Interface")]
            [Index(2)]
            public string UsesSoftwareSystemInterface { get; set; }

            [Name("Uses Container Interface")]
            [Index(3)]
            public string UsesContainerInterface { get; set; }

            [Name("Condition")]
            [Index(4)]
            public string Condition { get; set; }
        }

        public record Container
        {
            [Name("Software System")]
            [Index(1)]
            public string SoftwareSystem { get; set; }

            [Name("Name")]
            [Index(2)]
            public string Name { get; set; }

            [Name("Type")]
            [Index(3)]
            public string Type { get; set; }

            [Name("Alias")]
            [Index(4)]
            public string Alias { get; set; }

            public IEnumerable<ContainerAttribute> WithAttributes(CsvDataProvider dataProvider)
            {
                return dataProvider.ContainerAttributeRecords.Where(x => !string.IsNullOrEmpty(x.Container.Trim()) &&
                    x.Container == Alias);
            }
            public IEnumerable<ContainerInterface> WithInterfaces(CsvDataProvider dataProvider)
            {
                return dataProvider.ContainerInterfaceRecords.Where(x => x.Container == Alias);
            }
        }

        public record ContainerAttribute
        {
            [Name("Container")]
            [Index(1)]
            public string Container { get; set; }

            [Name("Attribute")]
            [Index(2)]
            public string Attribute { get; set; }

            [Name("Value")]
            [Index(3)]
            public string Value { get; set; }

            public bool TryGetAttributeName(CsvDataProvider dataProvider, out string? name)
            {
                name = dataProvider.AttributeRecords.FirstOrDefault(x => x.Alias == Attribute)?.Name;
                return name != null;
            }
        }

        public record ContainerInterface
        {
            [Name("Container")]
            [Index(1)]
            public string Container { get; set; }

            [Name("Name")]
            [Index(2)]
            public string Name { get; set; }

            [Name("Alias")]
            [Index(3)]
            public string Alias { get; set; }
            

            public IEnumerable<ContainerInterfaceFlow> WithUses(CsvDataProvider dataProvider)
            {
                return dataProvider.ContainerInterfaceFlowRecords.Where(x => x.ContainerInterface == Alias);
            }
        }

        public record ContainerInterfaceFlow
        {
            [Name("Container Interface")]
            [Index(1)]
            public string ContainerInterface { get; set; }

            [Name("Uses Container Interface")]
            [Index(2)]
            public string UsesContainerInterface { get; set; }

            [Name("Uses Software System Interface")]
            [Index(3)]
            public string UsesSoftwareSystemInterface { get; set; }

            [Name("Condition")]
            [Index(4)]
            public string Condition { get; set; }
        }

        public record ActorType
        {
            [Name("Name")]
            [Index(1)]
            public string Name { get; set; }

            [Name("Type")]
            [Index(2)]
            public string Type { get; set; }

        }

        public record Actor
        {
            [Name("Name")]
            [Index(1)]
            public string Name { get; set; }

            [Name("Type")]
            [Index(2)]
            public string TypeName { get; set; }

            [Name("Alias")]
            [Index(3)]
            public string Alias { get; set; }
            public bool TryGetType(CsvDataProvider dataProvider, out string? type)
            {
                type = dataProvider.ActorTypeRecords.FirstOrDefault(x => x.Name == TypeName)?.Type;
                return type != null;
            }
        }
        public record BusinessProcess
        {
            [Name("Name")]
            [Index(1)]
            public string Name { get; set; }

            [Name("Alias")]
            [Index(2)]
            public string Alias { get; set; }
            public IEnumerable<Activity> WithBusinessActivities(CsvDataProvider DataProvider)
            {
                return DataProvider.ActivityRecords.Where(x => !string.IsNullOrEmpty(x.BusinessProcess.Trim()) &&
                    x.BusinessProcess == Alias);
            }
        }

        public record Activity
        {
            [Name("Business Process")]
            [Index(1)]
            public string BusinessProcess { get; set; }

            [Name("Name")]
            [Index(2)]
            public string Name { get; set; }

            [Name("Actor")]
            [Index(3)]
            public string Actor { get; set; }

            [Name("Uses Container Interface")]
            [Index(4)]
            public string UsesContainerInterface { get; set; }

            [Name("Uses Software System Interface")]
            [Index(5)]
            public string UsesSoftwareSystemInterface { get; set; }


        }

        public record Attribute
        {
            [Name("Name")]
            [Index(1)]
            public string Name { get; set; }

            [Name("Alias")]
            [Index(2)]
            public string Alias { get; set; }
        }
    }
}
