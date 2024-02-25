using C4InterFlow.Structures;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Reflection.Metadata.BlobBuilder;

namespace C4InterFlow.Automation.Writers
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

        private JObject BuildActorsObject(string architectureNamespace, out string path)
        {
            path = $"{architectureNamespace}.Actors";
            return BuildObject(path);
        }

        protected JObject BuildActorObject(string architectureNamespace, string type, string name, string label, string? description = null)
        {
            var result = BuildActorsObject(architectureNamespace, out var path);
            var currentObject = result.SelectToken(path) as JObject;

            currentObject.Add(
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
            var path = AnyCodeWriter.GetContainerAlias(architectureNamespace, softwareSystemName, name);
            var result = BuildObject(path);
            var currentObject = result.SelectToken(path) as JObject;

            currentObject.Add("Label", label);
            currentObject.Add("Description", description != null ? description : string.Empty);
            currentObject.Add("ContainerType", type != null ? type : "None");
            currentObject.Add("Boundary", boundary != null ? boundary : "Internal");
            currentObject.Add("Technology", technology != null ? technology : string.Empty);
            currentObject.Add("Components", new JObject());
            currentObject.Add("Interfaces", new JObject());
            currentObject.Add("Entities", new JObject());

            return result;
        }

        protected JObject BuildComponentObject(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string componentType = "None", string? description = null, string? technology = null)
        {
            var path = AnyCodeWriter.GetComponentAlias(architectureNamespace, softwareSystemName, containerName, name);
            var result = BuildObject(path);
            var currentObject = result.SelectToken(path) as JObject;

            currentObject.Add("Label", label);
            currentObject.Add("ComponentType", componentType);
            currentObject.Add("Description", description != null ? description : string.Empty);
            currentObject.Add("Technology", technology != null ? technology : string.Empty);
            currentObject.Add("Interfaces", new JObject());

            return result;
        }

        public JObject BuildEntityObject(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string? type = null, string? description = null, string[]? composedOfMany = null, string[]? composedOfOne = null, string? extends = null)
        {
            var path = AnyCodeWriter.GetEntityAlias(architectureNamespace, softwareSystemName, containerName, name);
            var result = BuildObject(path);
            var currentObject = result.SelectToken(path) as JObject;

            currentObject.Add("Label", label);
            currentObject.Add("Description", description != null ? description : string.Empty);
            currentObject.Add("ComposedOfMany", composedOfMany != null ? new JArray(composedOfMany) : null);
            currentObject.Add("ComposedOfOne", composedOfOne != null ? new JArray(composedOfOne) : null);
            currentObject.Add("Extends", extends != null ? extends : string.Empty);

            return result;
        }

        public JObject BuildComponentInterfaceObject(string architectureNamespace, string softwareSystemName, string containerName, string componentName, string name, string label, string? description = null, string? protocol = null, string? path = null, bool? isPrivate = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null)
        {
            var componentAlias = AnyCodeWriter.GetComponentAlias(architectureNamespace, softwareSystemName, containerName, componentName);
            var componentInterfaceAlias = AnyCodeWriter.GetComponentInterfaceAlias(componentAlias, name);
            var result = BuildObject(componentInterfaceAlias);
            var currentObject = result.SelectToken(componentInterfaceAlias) as JObject;

            currentObject.Add("Label", label);
            currentObject.Add("Description", description != null ? description : string.Empty);
            currentObject.Add("Path", path != null ? path : string.Empty);
            currentObject.Add("IsPrivate", isPrivate != null ? isPrivate.Value : false);
            currentObject.Add("Protocol", protocol != null ? protocol : string.Empty);
            currentObject.Add("Flow", new JObject());
            currentObject.Add("Input", input != null ? input : string.Empty);
            currentObject.Add("InputTemplate", inputTemplate != null ? inputTemplate : string.Empty);
            currentObject.Add("Output", output != null ? output : string.Empty);
            currentObject.Add("OutputTemplate", outputTemplate != null ? outputTemplate : string.Empty);

            return result;
        }

        public JObject BuildContainerInterfaceObject(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string? description = null, string? protocol = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null)
        {
            var containerInterfaceAlias = AnyCodeWriter.GetContainerInterfaceAlias(architectureNamespace, softwareSystemName, containerName, name);
            var result = BuildObject(containerInterfaceAlias);
            var currentObject = result.SelectToken(containerInterfaceAlias) as JObject;

            currentObject.Add("Label", label);
            currentObject.Add("Description", description != null ? description : string.Empty);
            currentObject.Add("Protocol", protocol != null ? protocol : string.Empty);
            currentObject.Add("Flow", new JObject());
            currentObject.Add("Input", input != null ? input : string.Empty);
            currentObject.Add("InputTemplate", inputTemplate != null ? inputTemplate : string.Empty);
            currentObject.Add("Output", output != null ? output : string.Empty);
            currentObject.Add("OutputTemplate", outputTemplate != null ? outputTemplate : string.Empty);

            return result;
        }

        public JObject BuildSoftwareSystemInterfaceObject(string architectureNamespace, string softwareSystemName, string name, string label, string? description = null, string? protocol = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null)
        {
            var softwareSystemInterfaceAlias = AnyCodeWriter.GetSoftwareSystemInterfaceAlias(architectureNamespace, softwareSystemName, name);
            var result = BuildObject(softwareSystemInterfaceAlias);
            var currentObject = result.SelectToken(softwareSystemInterfaceAlias) as JObject;

            currentObject.Add("Label", label);
            currentObject.Add("Description", description != null ? description : string.Empty);
            currentObject.Add("Protocol", protocol != null ? protocol : string.Empty);
            currentObject.Add("Flow", new JObject());
            currentObject.Add("Input", input != null ? input : string.Empty);
            currentObject.Add("InputTemplate", inputTemplate != null ? inputTemplate : string.Empty);
            currentObject.Add("Output", output != null ? output : string.Empty);
            currentObject.Add("OutputTemplate", outputTemplate != null ? outputTemplate : string.Empty);

            return result;
        }

        public JObject BuildBusinessProcessObject(string architectureNamespace, string name, string label, JArray businessActivities, string? description = null)
        {
            var path = AnyCodeWriter.GetBusinessProcessAlias(architectureNamespace, name);
            var result = BuildObject(path);
            var currentObject = result.SelectToken(path) as JObject;

            currentObject.Add("Label", label);
            currentObject.Add("Description", description != null ? description : string.Empty);
            currentObject.Add("Activities", businessActivities);

            return result;
        }

        public JObject BuildFlowObject(Flow flow, string? owner = null)
        {
            var result = new JObject();

            if(!string.IsNullOrEmpty(owner))
            {
                result.Add("Owner", owner);
            }

            if(flow.Flows != null)
            {
                var flowsArray = new JArray();

                foreach (var innerFlow in flow.Flows)
                {
                    flowsArray.Add(BuildFlowObject(innerFlow, owner));
                }

                if(flowsArray.Count > 0)
                {
                    result.Add("Flows", flowsArray);
                }
            }
            return result;
        }
        public JObject BuildBusinessProcessActivityObject(string label, string actor, Flow[] flows)
        {
            var flowsArray = new JArray();

            foreach(var flow in flows)
            {
                flowsArray.Add(BuildFlowObject(flow, actor));
            }

            var activityFlow = new JObject { { "Owner", actor } };

            if(flowsArray.Count > 0)
            {
                activityFlow.Add("Flows", flowsArray);
            }

            var result = new JObject
            {
                { "Flow", activityFlow },
                { "Label", label }
            };

            return result;
        }
    }
}
