﻿using C4InterFlow.Structures;
using Serilog;

namespace C4InterFlow.Automation.Writers
{
    public class CsvToCSharpAaCGenerator : CsvToCSharpAaCWriterStrategy
    {
        public override void Execute()
        {
            var addSystemClassAction = "add System Class";
            var addSystemInterfaceClassAction = "add System Interface Class";

            var architectureRootNamespaceSegments = ArchitectureRootNamespace.Split('.');
            var writer = CsvToCSharpAaCWriter
                .WithCsvData(ArchitectureInputPath)
                .WithArchitectureRootNamespace(ArchitectureRootNamespace)
                .WithArchitectureProject(ArchitectureOutputPath);

            writer.WithSoftwareSystems()
                    .ToList().ForEach(s =>
                    {
                        Log.Information("Generating AaC for {Alias} Software System", s.Alias);

                        var softwareSystemName = s.Alias;
                        writer.AddSoftwareSystem(softwareSystemName, s.GetBoundary(), s.Name, s.Description);

                        s.WithInterfaces(writer.DataProvider).ToList().ForEach(i =>
                        {
                            writer.AddSoftwareSystemInterface(softwareSystemName, i.Alias.Split('.').Last(), i.Name, i.Description, protocol: i.Protocol);
                        });

                        s.WithContainers(writer.DataProvider).ToList().ForEach(c =>
                        {
                            Log.Information("Generating AaC for {Alias} container", c.Alias);
                            
                            var containerName = c.Alias.Split('.').Last();
                            writer.AddContainer(softwareSystemName, containerName, c.Type, c.Name, c.Description, c.Technology);

                            c.WithInterfaces(writer.DataProvider).ToList().ForEach(i =>
                            {
                                writer.AddContainerInterface(softwareSystemName, containerName, i.Alias.Split('.').Last(), i.Name, i.Description, protocol: i.Protocol);
                            });
                        });

                        Log.Information("Generating AaC flows for {Alias} Software System", s.Alias);

                        writer.WithSoftwareSystemInterfaceClasses(s.Alias, true)
                        .ToList().ForEach(x => x.AddFlowToSoftwareSystemInterfaceClass(
                            writer));

                        writer.WithContainerInterfaceClasses()
                        .ToList().ForEach(x => x.AddFlowToContainerInterfaceClass(
                            writer));

                    });

            Log.Information("Generating Actors");

            writer.WithActors()
                    .ToList().ForEach(a =>
                    {
                        if (!a.TryGetType(writer.DataProvider, out var type))
                        {
                            type = nameof(Person);
                        }
                        writer.AddActor(a.Alias, type, a.Name);
                    });
            Log.Information("Generating Business Processes");
            
            writer.WithBusinessProcesses()
                .ToList().ForEach(b => writer.AddBusinessProcess(b.Alias, b.Name));
        }
    }
}
