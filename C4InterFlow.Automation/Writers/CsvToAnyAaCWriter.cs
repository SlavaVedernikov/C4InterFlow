using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Globalization;

namespace C4InterFlow.Automation.Writers
{
    public abstract class CsvToAnyAaCWriter
    {
        protected const string FILE_SOFTWARE_SYSTEMS = "Software Systems";
        protected const string FILE_SOFTWARE_SYSTEM_INTERFACES = "Software System Interfaces";
        protected const string FILE_SOFTWARE_SYSTEM_INTERFACES_USE = "Software System Interfaces Use";
        protected const string FILE_CONTAINERS = "Containers";
        protected const string FILE_CONTAINER_INTERFACES = "Container Interfaces";
        protected const string FILE_CONTAINER_INTERFACES_USE = "Container Interfaces Use";
        protected const string FILE_ACTORS = "Actors";
        protected const string FILE_ACTOR_TYPES = "Actor Types";
        protected const string FILE_BUSINESS_PROCESSES = "Business Processes";
        protected const string FILE_BUSINESS_PROCESS_ACTIVITIES = "Business Process Activities";
        protected string? ArchitectureInputPath { get; set; }
        public string ArchitectureNamespace { get; protected set; }

        protected Actor[] ActorRecords { get; set; }
        protected ActorType[] ActorTypeRecords { get; set; }
        protected SoftwareSystem[] SoftwareSystemRecords { get; set; }
        protected SoftwareSystemInterface[] SoftwareSystemInterfaceRecords { get; set; }
        protected SoftwareSystemInterfaceUses[] SoftwareSystemInterfaceUsesRecords { get; set; }
        protected Container[] ContainerRecords { get; set; }
        protected ContainerInterface[] ContainerInterfaceRecords { get; set; }
        protected ContainerInterfaceUses[] ContainerInterfaceUsesRecords { get; set; }
        protected BusinessProcess[] BusinessProcessRecords { get; set; }
        protected Activity[] BusinessActivityRecords { get; set; }
        public Dictionary<string, SoftwareSystemInterface> SoftwareSystemInterfaceClassFileNameMap { get; private set; } = new Dictionary<string, SoftwareSystemInterface>();
        public Dictionary<string, ContainerInterface> ContainerInterfaceClassFileNameMap { get; private set; } = new Dictionary<string, ContainerInterface>();

        protected void LoadData(string architectureInputPath)
        {
            ArchitectureInputPath = architectureInputPath;

            Console.WriteLine($"Loading data from '{ArchitectureInputPath}'...");

            using (var reader = new StreamReader(@$"{ArchitectureInputPath}\{FILE_ACTORS}.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                ActorRecords = csv.GetRecords<Actor>().ToArray();
            }

            using (var reader = new StreamReader(@$"{ArchitectureInputPath}\{FILE_ACTOR_TYPES}.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                ActorTypeRecords = csv.GetRecords<ActorType>().ToArray();
            }

            using (var reader = new StreamReader(@$"{ArchitectureInputPath}\{FILE_SOFTWARE_SYSTEMS}.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                SoftwareSystemRecords = csv.GetRecords<SoftwareSystem>().ToArray();
            }

            using (var reader = new StreamReader(@$"{ArchitectureInputPath}\{FILE_SOFTWARE_SYSTEM_INTERFACES}.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                SoftwareSystemInterfaceRecords = csv.GetRecords<SoftwareSystemInterface>().ToArray();
            }

            using (var reader = new StreamReader(@$"{ArchitectureInputPath}\{FILE_SOFTWARE_SYSTEM_INTERFACES_USE}.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                SoftwareSystemInterfaceUsesRecords = csv.GetRecords<SoftwareSystemInterfaceUses>().ToArray();
            }

            using (var reader = new StreamReader(@$"{ArchitectureInputPath}\{FILE_CONTAINERS}.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                ContainerRecords = csv.GetRecords<Container>().ToArray();
            }

            using (var reader = new StreamReader(@$"{ArchitectureInputPath}\{FILE_CONTAINER_INTERFACES}.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                ContainerInterfaceRecords = csv.GetRecords<ContainerInterface>().ToArray();
            }

            using (var reader = new StreamReader(@$"{ArchitectureInputPath}\{FILE_CONTAINER_INTERFACES_USE}.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                ContainerInterfaceUsesRecords = csv.GetRecords<ContainerInterfaceUses>().ToArray();
            }

            using (var reader = new StreamReader(@$"{ArchitectureInputPath}\{FILE_ACTORS}.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                ActorRecords = csv.GetRecords<Actor>().ToArray();
            }

            using (var reader = new StreamReader(@$"{ArchitectureInputPath}\{FILE_BUSINESS_PROCESSES}.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                BusinessProcessRecords = csv.GetRecords<BusinessProcess>().ToArray();
            }

            using (var reader = new StreamReader(@$"{ArchitectureInputPath}\{FILE_BUSINESS_PROCESS_ACTIVITIES}.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                BusinessActivityRecords = csv.GetRecords<Activity>().ToArray();
            }
        }
        public class Actor
        {
            [Name("Actor Alias")]
            public string Alias { get; set; }
            [Name("Name")]
            public string Name { get; set; }
            [Name("Type")]
            public string TypeName { get; set; }
            public bool TryGetType(CsvToAnyAaCWriter writer, out string? type)
            {
                type = writer.ActorTypeRecords.FirstOrDefault(x => x.Name == TypeName)?.Type;
                return type != null;
            }
        }

        public class ActorType
        {
            [Name("Name")]
            public string Name { get; set; }
            [Name("Type")]
            public string Type { get; set; }
        }

        public class SoftwareSystem
        {
            [Name("Software System Alias")]
            public string Alias { get; set; }
            [Name("Name")]
            public string Name { get; set; }
            [Name("Is External")]
            [BooleanTrueValues("Yes", "yes")]
            [BooleanFalseValues("No", "no", "")]
            public bool IsExternal { get; set; }
            public string GetBoundary()
            {
                return IsExternal ? "External" : "Internal";
            }

            public IEnumerable<SoftwareSystemInterface> WithInterfaces(CsvToAnyAaCWriter writer)
            {
                return writer.SoftwareSystemInterfaceRecords.Where(x => !string.IsNullOrEmpty(x.SoftwareSystemAlias.Trim()) &&
                    x.SoftwareSystemAlias == Alias);
            }

            public IEnumerable<Container> WithContainers(CsvToAnyAaCWriter writer)
            {
                return writer.ContainerRecords.Where(x => !string.IsNullOrEmpty(x.SoftwareSystemAlias.Trim()) &&
                    x.SoftwareSystemAlias == Alias);
            }
        }

        public class SoftwareSystemInterface
        {
            [Name("Software System Alias")]
            public string SoftwareSystemAlias { get; set; }
            [Name("Alias")]
            public string Alias { get; set; }
            [Name("Name")]
            public string Name { get; set; }

            public IEnumerable<SoftwareSystemInterfaceUses> WithUses(CsvToAnyAaCWriter writer)
            {
                return writer.SoftwareSystemInterfaceUsesRecords.Where(x => !string.IsNullOrEmpty(x.InterfaceAlias.Trim()) &&
                    x.InterfaceAlias == Alias);
            }
        }

        public class SoftwareSystemInterfaceUses
        {
            [Name("Software System Interface Alias")]
            public string InterfaceAlias { get; set; }
            [Name("Uses Software System Interface Alias")]
            public string UsesSoftwareSystemInterfaceAlias { get; set; }
            [Name("Uses Container Interface Alias")]
            public string UsesContainerInterfaceAlias { get; set; }
            [Name("Condition")]
            public string Condition { get; set; }
        }

        public class Container
        {
            [Name("Software System Alias")]
            public string SoftwareSystemAlias { get; set; }
            [Name("Alias")]
            public string Alias { get; set; }
            [Name("Name")]
            public string Name { get; set; }
            [Name("Type")]
            public string Type { get; set; }

            public IEnumerable<ContainerInterface> WithInterfaces(CsvToAnyAaCWriter writer)
            {
                return writer.ContainerInterfaceRecords.Where(x => x.ContainerAlias == Alias);
            }
        }

        public class ContainerInterface
        {
            [Name("Container Alias")]
            public string ContainerAlias { get; set; }
            [Name("Alias")]
            public string Alias { get; set; }
            [Name("Name")]
            public string Name { get; set; }

            public IEnumerable<ContainerInterfaceUses> WithUses(CsvToAnyAaCWriter writer)
            {
                return writer.ContainerInterfaceUsesRecords.Where(x => x.InterfaceAlias == Alias);
            }
        }

        public class ContainerInterfaceUses
        {
            [Name("Container Interface Alias")]
            public string InterfaceAlias { get; set; }
            [Name("Uses Container Interface Alias")]
            public string UsesContainerInterfaceAlias { get; set; }
            [Name("Uses Software System Interface Alias")]
            public string UsesSoftwareSystemInterfaceAlias { get; set; }
            [Name("Condition")]
            public string Condition { get; set; }
        }

        public class BusinessProcess
        {
            [Name("Business Process Alias")]
            public string Alias { get; set; }
            [Name("Name")]
            public string Name { get; set; }

            public IEnumerable<Activity> WithBusinessActivities(CsvToAnyAaCWriter writer)
            {
                return writer.BusinessActivityRecords.Where(x => !string.IsNullOrEmpty(x.BusinessProcessAlias.Trim()) &&
                    x.BusinessProcessAlias == Alias);
            }
        }

        public class Activity
        {
            [Name("Business Process Alias")]
            public string BusinessProcessAlias { get; set; }

            [Name("Activity Name")]
            public string Name { get; set; }

            [Name("Actor Alias")]
            public string ActorAlias { get; set; }

            [Name("Uses Container Interface Alias")]
            public string UsesContainerInterfaceAlias { get; set; }

            [Name("Uses Software System Interface Alias")]
            public string UsesSoftwareSystemInterfaceAlias { get; set; }


        }
    }
}
