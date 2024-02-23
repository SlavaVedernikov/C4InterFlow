using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace C4InterFlow.Automation.Writers
{
    public static class CsvToJsonExtensions
    {
        public static JObject AddFlowToSoftwareSystemInterfaceObject(this JObject jsonObject,
            CsvToJsonAaCWriter writer)
        {
            var softwareSystemInterface = writer.SoftwareSystemInterfaceAaCPathToCsvRecordMap.GetValueOrDefault(jsonObject.Path);
            if (softwareSystemInterface == null || !softwareSystemInterface.WithUses(writer.DataProvider).Any()) return jsonObject;

            var flows = new JArray();

            softwareSystemInterface.WithUses(writer.DataProvider)
            .ToList().ForEach(i =>
            {
                if (!string.IsNullOrEmpty(i.UsesContainerInterface))
                {
                    flows.Add(new JObject
                            {
                                { "Type", "Use" },
                                { "Expression", $"{writer.ArchitectureNamespace}.SoftwareSystems.{i.UsesContainerInterface}" }
                            }
                    );
                }
                else if (!string.IsNullOrEmpty(i.UsesSoftwareSystemInterface))
                {
                    flows.Add(new JObject
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
            CsvToJsonAaCWriter writer)
        {
            var containerInterface = writer.ContainerInterfaceAaCPathToCsvRecordMap.GetValueOrDefault(jsonObject.Path);

            if (containerInterface == null || !containerInterface.WithUses(writer.DataProvider).Any()) return jsonObject;

            var flows = new JArray();

            containerInterface.WithUses(writer.DataProvider)
                .ToList().ForEach(i =>
                {

                    if (!string.IsNullOrEmpty(i.UsesContainerInterface))
                    {
                        flows.Add(new JObject
                            {
                                { "Type", "Use" },
                                { "Expression", $"{writer.ArchitectureNamespace}.SoftwareSystems.{i.UsesContainerInterface}" }
                            }
                        );
                    }
                    else if (!string.IsNullOrEmpty(i.UsesSoftwareSystemInterface))
                    {
                        flows.Add(new JObject
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
