using C4InterFlow.Structures;

namespace C4InterFlow.Automation.Writers
{
    public class CsvToJsonAaCGenerator : CsvToJsonAaCWriterStrategy
    {
        public override void Execute()
        {
            var addSystemClassAction = "add System Class";
            var addSystemInterfaceClassAction = "add System Interface Class";

            var architectureRootNamespaceSegments = ArchitectureRootNamespace.Split('.');
            var writer = CsvToJsonAaCWriter
                .WithCsvData(ArchitectureInputPath);


            writer.WithSoftwareSystems()
                    .ToList().ForEach(s =>
                    {

                        writer
                            .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                            .WithSoftwareSystemsCollection()
                            .WithArchitectureOutputPath(Path.Combine(ArchitectureOutputPath, "SoftwareSystems", $"{s.Alias}.json"));

                        var softwareSystemName = s.Alias;
                        writer.AddSoftwareSystem(softwareSystemName, s.GetBoundary(), s.Name);

                        s.WithInterfaces(writer.DataProvider).ToList().ForEach(i =>
                        {
                            writer.AddSoftwareSystemInterface(softwareSystemName, i.Alias.Split('.').Last(), i.Name);
                        });

                        s.WithContainers(writer.DataProvider).ToList().ForEach(c =>
                        {
                            var containerName = c.Alias.Split('.').Last();
                            writer.AddContainer(softwareSystemName, containerName, c.Type, c.Name);

                            c.WithInterfaces(writer.DataProvider).ToList().ForEach(i =>
                            {
                                writer.AddContainerInterface(softwareSystemName, containerName, i.Alias.Split('.').Last(), i.Name);
                            });
                        });

                        writer.WithSoftwareSystemInterfaceObjects(s.Alias)
                        .ToList().ForEach(x => x.AddFlowToSoftwareSystemInterfaceObject(
                            writer));

                        writer.WithContainerInterfaceObjects(s.Alias)
                        .ToList().ForEach(x => x.AddFlowToContainerInterfaceObject(
                            writer));

                        writer.WriteArchitecture();

                    });

            writer.WithActors()
                    .ToList().ForEach(a =>
                    {
                        writer
                            .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                            .WithActorsCollection()
                            .WithArchitectureOutputPath(Path.Combine(ArchitectureOutputPath, "Actors", $"{a.Alias}.json"));

                        if (!a.TryGetType(writer.DataProvider, out var type))
                        {
                            type = nameof(Person);
                        }

                        writer.AddActor(a.Alias, type, a.Name);

                        writer.WriteArchitecture();
                    });

            writer.WithBusinessProcesses()
                .ToList().ForEach(b =>
                {
                    writer
                        .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                        .WithBusinessProcessesCollection()
                        .WithArchitectureOutputPath(Path.Combine(ArchitectureOutputPath, "BusinessProcessess", $"{b.Alias}.json"));

                    writer.AddBusinessProcess(b.Alias, b.Name);

                    writer.WriteArchitecture();
                });
        }
    }
}
