using C4InterFlow.Structures;

namespace C4InterFlow.Automation.Writers
{
    public abstract class CsvToAnyAaCWriter : IAaCWriter
    {
        protected string? ArchitectureInputPath { get; set; }
        public string ArchitectureNamespace { get; protected set; }
        public Dictionary<string, CsvDataProvider.SoftwareSystem> SoftwareSystemAaCPathToCsvRecordMap { get; private set; } = new Dictionary<string, CsvDataProvider.SoftwareSystem>();
        public Dictionary<string, CsvDataProvider.SoftwareSystemInterface> SoftwareSystemInterfaceAaCPathToCsvRecordMap { get; private set; } = new Dictionary<string, CsvDataProvider.SoftwareSystemInterface>();
        public Dictionary<string, CsvDataProvider.ContainerInterface> ContainerInterfaceAaCPathToCsvRecordMap { get; private set; } = new Dictionary<string, CsvDataProvider.ContainerInterface>();

        public CsvDataProvider DataProvider { get; init; }
        protected CsvToAnyAaCWriter(string architectureInputPath)
        {
            ArchitectureInputPath = architectureInputPath;

            Console.WriteLine($"Reading data from '{ArchitectureInputPath}'...");

            DataProvider = new CsvDataProvider(ArchitectureInputPath);
        }

        protected IEnumerable<Structures.Activity> GetBusinessProcessActivities(CsvDataProvider.BusinessProcess businessProcess)
        {
            var result = new List<Structures.Activity>();

            if (businessProcess == null) return result;

            foreach (var businessActivity in businessProcess.WithBusinessActivities(DataProvider)
                .Where(x => !string.IsNullOrEmpty(x.UsesSoftwareSystemInterface) ||
                    !string.IsNullOrEmpty(x.UsesContainerInterface))
                .GroupBy(x => new { x.Name, x.Actor })
                .Select(g => new
                {
                    g.Key.Name,
                    g.Key.Actor,
                    Uses = g.Select(x => $"{ArchitectureNamespace}.SoftwareSystems.{(string.IsNullOrEmpty(x.UsesContainerInterface) ? x.UsesSoftwareSystemInterface : x.UsesContainerInterface)}").ToArray()
                }))
            {
                var flow = new Flow();

                foreach(var usesInterface in businessActivity.Uses)
                {
                    flow.Use(usesInterface);
                }

                result.Add(new Activity(
                    flow, 
                    $"{ArchitectureNamespace}.Actors.{businessActivity.Actor}", 
                    businessActivity.Name));
            }

            return result;
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
