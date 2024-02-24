using C4InterFlow.Structures;
using System;

namespace C4InterFlow.Automation.Writers
{
    public class YamlToCsvAaCGenerator : YamlToCsvAaCWriterStrategy
    {
        public override void Execute()
        {
            var architectureRootNamespaceSegments = ArchitectureRootNamespace.Split('.');
            var writer = new YamlToCsvAaCWriter(ArchitectureInputPath, ArchitectureOutputPath);

            writer
                .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                .WithSoftwareSystems().ToList().ForEach(s =>
            {
                var softwareSystemName = s.Alias.Split('.').Last();
                writer
                .AddSoftwareSystem(softwareSystemName, Enum.GetName(typeof(Boundary), s.Boundary), s.Label);

                writer.WithInterfaces(s).ToList().ForEach(i =>
                {
                    writer.AddSoftwareSystemInterface(s.Name, i.Name, i.Label);
                    writer.AddSoftwareSystemInterfaceFlows(i);
                });

                writer.WithContainers(s).ToList().ForEach(c =>
                {
                    var containerName = c.Alias.Split('.').Last();
                    writer.AddContainer(softwareSystemName, containerName, Enum.GetName(typeof(ContainerType), c.ContainerType), c.Label);

                    writer.WithInterfaces(c).ToList().ForEach(i =>
                    {
                        writer.AddContainerInterface(softwareSystemName, containerName, i.Name, i.Label);
                        writer.AddContainerInterfaceFlows(i);
                    });
                });
            });

            writer.WithActors().ToList().ForEach(a =>
            {
                writer.AddActor(a.Alias.Split('.').Last(), a.GetType().Name, a.Label);
            });

            writer.WithBusinessProcesses().ToList().ForEach(b =>
            {
                var businessProcessName = b.Alias.Split(".").Last();
                writer.AddBusinessProcess(businessProcessName, b.Label);

                foreach(var activity in b.Activities)
                {
                    writer.AddActivity(
                        businessProcessName,
                        activity.Actor.Split('.').Last(),
                        activity.Flow.Flows,
                        activity.Label);
                }
            });

            writer.WriteArchitecture();
        }
    }
}
