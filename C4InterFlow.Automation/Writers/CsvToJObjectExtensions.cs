using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using System.Reflection.Metadata;

namespace C4InterFlow.Automation.Writers
{
    public static class CsvToJObjectExtensions
    {
        public static JObject AddAttributesToSoftwareSystemObject(this JObject jsonObject,
            CsvToJObjectAaCWriter writer)
        {
            var softwareSystem = writer.SoftwareSystemAaCPathToCsvRecordMap.GetValueOrDefault(jsonObject.Path);
            if (softwareSystem == null || !softwareSystem.WithAttributes(writer.DataProvider).Any()) return jsonObject;

            var attributes = new JObject();

            softwareSystem.WithAttributes(writer.DataProvider)
            .ToList().ForEach(a =>
            {
                attributes.Add(a.Attribute, a.Value );
            });

            jsonObject.Add("Attributes", attributes);

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

            jsonObject.Add("Flow", new JObject
                        {
                            { "Flows", flows }
                        });

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

            jsonObject.Add("Flow", new JObject
                        {
                            { "Flows", flows }
                        });

            return jsonObject;
        }
    }
}
