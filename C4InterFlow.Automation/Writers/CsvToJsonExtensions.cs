using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace C4InterFlow.Automation.Writers
{
    public static class CsvToJsonExtensions
    {
        public static JObject AddFlowToSoftwareSystemInterfaceObject(this JObject jsonObject,
            CsvToJsonAaCWriter writer)
        {
            var softwareSystemInterface = writer.SoftwareSystemInterfaceClassFileNameMap.GetValueOrDefault(jsonObject.Path);
            if (softwareSystemInterface == null || !softwareSystemInterface.WithUses(writer).Any()) return jsonObject;

            var flows = new JArray();

            softwareSystemInterface.WithUses(writer)
            .ToList().ForEach(i =>
            {
                if (!string.IsNullOrEmpty(i.UsesContainerInterfaceAlias))
                {
                    flows.Add(new JObject
                            {
                                { "Type", "Use" },
                                { "Expression", $"{writer.ArchitectureNamespace}.SoftwareSystems.{i.UsesContainerInterfaceAlias}" }
                            }
                    );
                }
                else if (!string.IsNullOrEmpty(i.UsesSoftwareSystemInterfaceAlias))
                {
                    flows.Add(new JObject
                        {
                            { "Type", "Use" },
                            { "Expression", $"{writer.ArchitectureNamespace}.SoftwareSystems.{i.UsesSoftwareSystemInterfaceAlias}" }
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
            var containerInterface = writer.ContainerInterfaceClassFileNameMap.GetValueOrDefault(jsonObject.Path);

            if (containerInterface == null || !containerInterface.WithUses(writer).Any()) return jsonObject;

            var flows = new JArray();

            containerInterface.WithUses(writer)
                .ToList().ForEach(i =>
                {

                    if (!string.IsNullOrEmpty(i.UsesContainerInterfaceAlias))
                    {
                        flows.Add(new JObject
                            {
                                { "Type", "Use" },
                                { "Expression", $"{writer.ArchitectureNamespace}.SoftwareSystems.{i.UsesContainerInterfaceAlias}" }
                            }
                        );
                    }
                    else if (!string.IsNullOrEmpty(i.UsesSoftwareSystemInterfaceAlias))
                    {
                        flows.Add(new JObject
                            {
                                { "Type", "Use" },
                                { "Expression", $"{writer.ArchitectureNamespace}.SoftwareSystems.{i.UsesSoftwareSystemInterfaceAlias}" }
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
