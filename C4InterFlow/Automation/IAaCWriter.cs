using C4InterFlow.Structures;

namespace C4InterFlow.Automation
{
    public interface IAaCWriter
    {
        public IAaCWriter AddActor(string name, string type, string? label = null);
        public IAaCWriter AddBusinessProcess(string name, string? label = null);
        public IAaCWriter AddSoftwareSystem(string name, string? boundary = null, string? label = null, string? description = null);
        public IAaCWriter AddSoftwareSystemInterface(
            string softwareSystemName,
            string name,
            string? label = null,
            string? description = null,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null);

        public IAaCWriter AddContainer(string softwareSystemName, string name, string? containerType = null, string? label = null, string? description = null);

        public IAaCWriter AddContainerInterface(
            string softwareSystemName,
            string containerName,
            string name,
            string? label = null,
            string? description = null,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null);

        public IAaCWriter AddComponent(string softwareSystemName, string containerName, string name, ComponentType componentType = ComponentType.None);

        public IAaCWriter AddComponentInterface(
            string softwareSystemName,
            string containerName,
            string componentName,
            string name,
            string? label = null,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null);
    }
}
