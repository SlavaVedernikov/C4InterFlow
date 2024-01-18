using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace C4InterFlow.Automation
{
    public abstract class JObjectBuilder
    {
        public abstract string GetCode(JObject jObject);

        private JObject BuildObject(string path)
        {
            return BuildObject(new JObject(), path);
        }
        private JObject BuildObject(JObject root, string path)
        {
            var pathSegments = path.Split('.');
            var result = root;
            var currentSegmentObject = result;
            foreach (var segment in pathSegments)
            {
                currentSegmentObject = AddObjectProperty(currentSegmentObject, segment);
            }

            return result;
        }

        private JObject AddObjectProperty(JObject root, string name)
        {
            root.Add(name, new JObject());
            return root.SelectToken(name) as JObject;
        }


        private JObject BuildSoftwareSystemsObject(string architectureNamespace, out string path)
        {
            path = $"{architectureNamespace}.SoftwareSystems";
            return BuildObject(path);
        }

        private JObject BuildActorsObject(string architectureNamespace)
        {
            return BuildObject($"{architectureNamespace}.Actors");
        }

        private JObject BuildusinessProcessesObject(string architectureNamespace)
        {
            return BuildObject($"{architectureNamespace}.BusinessProcesses");
        }

        protected JObject BuildActorObject(string architectureNamespace, string type, string name, string label, string? description = null)
        {
            var result = BuildActorsObject(architectureNamespace);

            result.Add(
                AnyCodeWriter.GetName(name),
                new JObject
                {
                    { "Type", type },
                    { "Label", label },
                    { "Description", description != null ? description : string.Empty },
                }
            );

            return result;
        }

        protected JObject BuildSoftwareSystemObject(string architectureNamespace, string name, string label, string? description = null, string? boundary = null)
        {
            var result = BuildSoftwareSystemsObject(architectureNamespace, out var path);

            var currentObject = result.SelectToken(path) as JObject;
            currentObject.Add(
                AnyCodeWriter.GetName(name),
                new JObject
                {
                    { "Label", label },
                    { "Boundary", boundary != null ? boundary : "Internal" },
                    { "Description", description != null ? description : string.Empty },
                    { "Containers", new JObject() },
                    { "Interfaces", new JObject() }
                }
            );

            return result;
        }

        protected JObject BuildContainerObject(string architectureNamespace, string softwareSystemName, string name, string label, string? type = null, string? description = null, string? technology = null, string? boundary = null)
        {
            var result = BuildObject(AnyCodeWriter.GetContainerAlias(architectureNamespace, softwareSystemName, name));

            result.Add("Label", label);
            result.Add("Description", description != null ? description : string.Empty);
            result.Add("ContainerType", type != null ? type : "None");
            result.Add("Boundary", boundary != null ? boundary : "Internal");
            result.Add("Technology", technology != null ? technology : string.Empty);
            result.Add("Components", new JObject());
            result.Add("Interfaces", new JObject());
            result.Add("Entities", new JObject());

            return result;
        }

        protected JObject BuildComponentObject(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string componentType = "None", string? description = null, string? technology = null)
        {
            var result = BuildObject(AnyCodeWriter.GetComponentAlias(architectureNamespace, softwareSystemName, containerName, name));

            result.Add("Label", label);
            result.Add("ComponentType", componentType);
            result.Add("Description", description != null ? description : string.Empty);
            result.Add("Technology", technology != null ? technology : string.Empty);
            result.Add("Containers", new JObject());

            return result;
        }

        public JObject BuildEntityObject(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string? type = null, string? description = null, string[]? composedOfMany = null, string[]? composedOfOne = null, string? extends = null)
        {
            var result = BuildObject(AnyCodeWriter.GetEntityAlias(architectureNamespace, softwareSystemName, containerName, name));

            result.Add("Label", label);
            result.Add("Description", description != null ? description : string.Empty);
            result.Add("ComposedOfMany", composedOfMany != null ? new JArray(composedOfMany) : null);
            result.Add("ComposedOfOne", composedOfOne != null ? new JArray(composedOfOne) : null);
            result.Add("Extends", extends != null ? extends : string.Empty);

            return result;
        }

        public JObject BuildComponentInterfaceObject(string architectureNamespace, string softwareSystemName, string containerName, string componentName, string name, string label, string? description = null, string? protocol = null, string? path = null, bool? isPrivate = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null)
        {
            var componentAlias = AnyCodeWriter.GetComponentAlias(architectureNamespace, softwareSystemName, containerName, componentName);
            var componentInterfaceAlias = AnyCodeWriter.GetComponentInterfaceAlias(componentAlias, name);
            var result = BuildObject(componentInterfaceAlias);

            result.Add("Label", label);
            result.Add("Description", description != null ? description : string.Empty);
            result.Add("Path", path != null ? path : string.Empty);
            result.Add("IsPrivate", isPrivate != null ? isPrivate.Value : false);
            result.Add("Protocol", protocol != null ? protocol : string.Empty);
            result.Add("Flow", new JObject { { "Owner", componentInterfaceAlias } });
            result.Add("Input", input != null ? input : string.Empty);
            result.Add("InputTemplate", inputTemplate != null ? inputTemplate : string.Empty);
            result.Add("Output", output != null ? output : string.Empty);
            result.Add("OutputTemplate", outputTemplate != null ? outputTemplate : string.Empty);

            return result;
        }

        public JObject BuildContainerInterfaceObject(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string? description = null, string? protocol = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null)
        {
            var containerInterfaceAlias = AnyCodeWriter.GetContainerInterfaceAlias(architectureNamespace, softwareSystemName, containerName, name);
            var result = BuildObject(containerInterfaceAlias);

            result.Add("Label", label);
            result.Add("Description", description != null ? description : string.Empty);
            result.Add("Protocol", protocol != null ? protocol : string.Empty);
            result.Add("Flow", new JObject());
            result.Add("Input", input != null ? input : string.Empty);
            result.Add("InputTemplate", inputTemplate != null ? inputTemplate : string.Empty);
            result.Add("Output", output != null ? output : string.Empty);
            result.Add("OutputTemplate", outputTemplate != null ? outputTemplate : string.Empty);

            return result;
        }

        public JObject BuildSoftwareSystemInterfaceObject(string architectureNamespace, string softwareSystemName, string name, string label, string? description = null, string? protocol = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null)
        {
            var softwareSystemInterfaceAlias = AnyCodeWriter.GetSoftwareSystemInterfaceAlias(architectureNamespace, softwareSystemName, name);
            var result = BuildObject(softwareSystemInterfaceAlias);

            result.Add("Label", label);
            result.Add("Description", description != null ? description : string.Empty);
            result.Add("Protocol", protocol != null ? protocol : string.Empty);
            result.Add("Flow", new JObject());
            result.Add("Input", input != null ? input : string.Empty);
            result.Add("InputTemplate", inputTemplate != null ? inputTemplate : string.Empty);
            result.Add("Output", output != null ? output : string.Empty);
            result.Add("OutputTemplate", outputTemplate != null ? outputTemplate : string.Empty);

            return result;
        }

        public JObject BuildBusinessProcessObject(string architectureNamespace, string name, string label, JArray businessActivities, string? description = null)
        {
            var result = BuildObject(AnyCodeWriter.GetBusinessProcessAlias(architectureNamespace, name));

            result.Add("Label", label);
            result.Add("Description", description != null ? description : string.Empty);
            result.Add("Activities", businessActivities);

            return result;
        }

        public JObject BuildBusinessActivityObject(string name, string actor, string[] uses)
        {
            return new JObject
            {
                { "Flow", new JObject {
                                            { "Owner", actor },
                                            { "Flows", new JArray(uses.Select(x => new JObject {
                                                    { "Type", "Use"},
                                                    { "Params", x}
                                                }).ToArray()) } } },
                { "Name", name }
            };
        }
    }
}
