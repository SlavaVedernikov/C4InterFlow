using C4InterFlow.Structures;
using System;

namespace C4InterFlow.Automation.Writers
{
    public class JsonToCSharpAaCGenerator : JsonToCSharpAaCWriterStrategy
    {
        public override void Execute()
        {
            var architectureRootNamespaceSegments = ArchitectureRootNamespace.Split('.');
            var writer = JsonToCSharpAaCWriter
                .WithJsonData(ArchitectureInputPath)
                .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                .WithArchitectureProject(ArchitectureOutputPath);

            writer.WithSoftwareSystems()
                    .ToList().ForEach(s =>
                    {
                        var softwareSystemName = s.Name;
                        writer
                        .AddSoftwareSystem(softwareSystemName, Enum.GetName(typeof(Boundary), s.Boundary), s.Label);

                        writer.WithInterfaces(s).ToList().ForEach(i =>
                        {
                            writer.AddSoftwareSystemInterface(s.Name, i.Name, i.Label);
                        });

                        writer.WithContainers(s).ToList().ForEach(c =>
                        {
                            writer.AddContainer(softwareSystemName, c.Name, Enum.GetName(typeof(ContainerType), c.ContainerType), c.Label, c.Description, c.Technology);

                            writer.WithInterfaces(c).ToList().ForEach(i =>
                            {
                                writer.AddContainerInterface(softwareSystemName, c.Name, i.Name, i.Label);
                            });
                        });

                        writer.WithSoftwareSystemInterfaceClasses(softwareSystemName, true)
                        .ToList().ForEach(x => x.AddFlowToSoftwareSystemInterfaceClass(
                            writer));

                        writer.WithContainerInterfaceClasses()
                        .ToList().ForEach(x => x.AddFlowToContainerInterfaceClass(
                            writer));

                    });

        }
    }
}
