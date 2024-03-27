using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using System.Reflection.Metadata;
using System.Text.Json.Nodes;

namespace C4InterFlow.Automation.Writers
{
    public static class CsvToJObjectExtensions
    {
        public static JObject AddAttributesToSoftwareSystemObject(this JObject jsonObject,
            CsvToJObjectAaCWriter writer)
        {
            var softwareSystem = writer.SoftwareSystemAaCPathToCsvRecordMap.GetValueOrDefault(jsonObject.Path);
            if (softwareSystem == null) return jsonObject;

            var softwareSystemAttributes = new JObject();

            softwareSystem.WithAttributes(writer.DataProvider)
            .ToList().ForEach(a =>
            {
                var attribute = new JObject();

                if(a.TryGetAttributeName(writer.DataProvider, out var name))
                {
                    attribute.Add("Label", name);
                }
                attribute.Add("Value", a.Value);

                softwareSystemAttributes.Add(a.Attribute, attribute);
            });

            if(softwareSystemAttributes.Children().Any())
            {
                jsonObject.Add("Attributes", softwareSystemAttributes);
            }

            softwareSystem.WithContainers(writer.DataProvider)
            .ToList().ForEach(c =>
            {
                var containerAttributes = new JObject();

                c.WithAttributes(writer.DataProvider)
                .ToList().ForEach(a =>
                {
                    var attribute = new JObject();

                    if (a.TryGetAttributeName(writer.DataProvider, out var name))
                    {
                        attribute.Add("Label", name);
                    }
                    attribute.Add("Value", a.Value);

                    containerAttributes.Add(a.Attribute, attribute);
                });

                var containerJObject = jsonObject.SelectToken($"Containers.{c.Alias.Split('.').Last()}") as JObject;

                if (containerJObject != null && containerAttributes.Children().Any())
                {
                    containerJObject.Add("Attributes", containerAttributes);
                }
            });

            return jsonObject;
        }
        public static JObject AddFlowToSoftwareSystemInterfaceObject(this JObject jsonObject,
            CsvToJObjectAaCWriter writer)
        {
            var softwareSystemInterface = writer.SoftwareSystemInterfaceAaCPathToCsvRecordMap.GetValueOrDefault(jsonObject.Path);
            if (softwareSystemInterface == null || !softwareSystemInterface.WithUses(writer.DataProvider).Any()) return jsonObject;

            var flows = new JArray();

            softwareSystemInterface.WithUses(writer.DataProvider)
            .ToList().ForEach(i =>
            {
                var currentFlows = flows;
                var hasCondition = !string.IsNullOrEmpty(i.Condition);

                if (hasCondition)
                {
                    var ifFlows = new JArray();
                    var ifFlow = new JObject
                    {
                        { "Type", "If" },
                        { "Expression", i.Condition },
                        { "Flows", ifFlows }
                    };

                    flows.Add(ifFlow);
                    currentFlows = ifFlows;
                }

                if (!string.IsNullOrEmpty(i.UsesContainerInterface))
                {
                    currentFlows.Add(new JObject
                            {
                                { "Type", "Use" },
                                { "Expression", $"{writer.ArchitectureNamespace}.SoftwareSystems.{i.UsesContainerInterface}" }
                            }
                    );
                }
                else if (!string.IsNullOrEmpty(i.UsesSoftwareSystemInterface))
                {
                    currentFlows.Add(new JObject
                        {
                            { "Type", "Use" },
                            { "Expression", $"{writer.ArchitectureNamespace}.SoftwareSystems.{i.UsesSoftwareSystemInterface}" }
                        }
                    );
                }
            });

            jsonObject.Add("Flows", flows);

            return jsonObject;
        }

        public static JObject AddFlowToContainerInterfaceObject(this JObject jsonObject,
            CsvToJObjectAaCWriter writer)
        {
            var containerInterface = writer.ContainerInterfaceAaCPathToCsvRecordMap.GetValueOrDefault(jsonObject.Path);

            if (containerInterface == null || !containerInterface.WithUses(writer.DataProvider).Any()) return jsonObject;

            var flows = new JArray();

            containerInterface.WithUses(writer.DataProvider)
                .ToList().ForEach(i =>
                {
                    var currentFlows = flows;
                    var hasCondition = !string.IsNullOrEmpty(i.Condition);

                    if (hasCondition)
                    {
                        var ifFlows = new JArray();
                        var ifFlow = new JObject
                    {
                        { "Type", "If" },
                        { "Expression", i.Condition },
                        { "Flows", ifFlows }
                    };

                        flows.Add(ifFlow);
                        currentFlows = ifFlows;
                    }

                    if (!string.IsNullOrEmpty(i.UsesContainerInterface))
                    {
                        currentFlows.Add(new JObject
                            {
                                { "Type", "Use" },
                                { "Expression", $"{writer.ArchitectureNamespace}.SoftwareSystems.{i.UsesContainerInterface}" }
                            }
                        );
                    }
                    else if (!string.IsNullOrEmpty(i.UsesSoftwareSystemInterface))
                    {
                        currentFlows.Add(new JObject
                            {
                                { "Type", "Use" },
                                { "Expression", $"{writer.ArchitectureNamespace}.SoftwareSystems.{i.UsesSoftwareSystemInterface}" }
                            }
                        );
                    }
                });

            jsonObject.Add("Flows", flows);

            return jsonObject;
        }
    }
}
