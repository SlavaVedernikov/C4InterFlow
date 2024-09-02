using C4InterFlow.Automation.Writers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetEShop.Cli
{
    public class CSharpToYamlCatalogApiAaCGenerator : CSharpToYamlAaCWriterStrategy
    {
        public override void Execute()
        {
            var addComponentObjectAction = "add Component Object";
            var addComponentInterfaceObjectAction = "add Component Interface Object";
            var addEntityObjectAction = "add Entity Object";

            var writer = new CSharpToYamlAaCWriter(SoftwareSystemSourcePath, ArchitectureRootNamespace, ArchitectureOutputPath);

            writer.AddSoftwareSystem(SoftwareSystemName);

            var projectName = string.Empty;
            var containerName = string.Empty;
            var componentName = string.Empty;

            projectName = "Catalog.API";
            containerName = "Api";

            writer.WithSoftwareSystemProject(projectName)
                .AddContainer(SoftwareSystemName, containerName);

            Console.WriteLine($"Adding {containerName} Container, Component and Interface objects for '{projectName}' project...");
            writer.WithDocuments().Where(d => d.FilePath.Contains(@"\Apis\") && d.Name.EndsWith("Api.cs"))
                .ToList().ForEach(d =>
                {
                    d.WithClasses().Where(c => c.Identifier.Text.EndsWith("Api"))
                    .ToList().ForEach(c =>
                    {
                        c.AddComponentYamlFile(SoftwareSystemName, containerName, writer)
                        .WithMethods()
                        .ToList().ForEach(m =>
                            m.AddComponentInterfaceYamlFile(
                                SoftwareSystemName,
                                containerName,
                                c.Identifier.Text,
                                writer));
                    });
                });

            containerName = "Infrastructure";

            Console.WriteLine($"Adding {containerName} Container, Component and Interface objects for '{projectName}' project...");
            writer.AddContainer(SoftwareSystemName, containerName);

            writer.WithDocuments().Where(d => d.FilePath.Contains(@"\Infrastructure\") && d.Name.EndsWith("Context.cs"))
                .ToList().ForEach(d =>
                {
                    d.WithClasses().Where(c => c.Identifier.Text.EndsWith("Context"))
                    .ToList().ForEach(c =>
                    {
                        c.AddComponentYamlFile(SoftwareSystemName, containerName, writer)
                        .WithProperties()
                        .ToList().ForEach(p =>
                            p.AddComponentInterfaceYamlFile(
                                SoftwareSystemName,
                                containerName,
                                c.Identifier.Text,
                                writer,
                                Utils.DbContextEntityInterfaces));

                        foreach (var @interface in Utils.DbContextInterfaces)
                        {
                            writer.AddComponentInterface(
                                SoftwareSystemName,
                                containerName,
                                c.Identifier.Text,
                                @interface);
                        }

                    });
                });


            Console.WriteLine($"Adding Software System Type Mappings...");
            AddSoftwareSystemTypeMapping(writer);

            Console.WriteLine($"Updating Flow property in all Interface objects...");
            writer.WithComponentInterfaceFiles().ToList()
                .ForEach(x => writer.AddFlowToComponentInterfaceYamlFile(
                    x,
                    null,
                    new NetToAnyAlternativeInvocationMapperConfig[]
                    {
                        new NetToAnyAlternativeInvocationMapperConfig() {
                            Mapper = Utils.MapDbContextEntityInvocation,
                            Args = new Dictionary<string, object>() {
                                { Utils.ARG_SOFTWARE_SYSTEM_NAME, SoftwareSystemName },
                                { Utils.ARG_INVOCATION_INTERFACES, Utils.DbContextEntityInterfaces },
                                { Utils.ARG_CONTAINER_NAME, "Infrastructure" }
                            }
                        },
                        new NetToAnyAlternativeInvocationMapperConfig() {
                            Mapper = Utils.MapDbContextInvocation,
                            Args = new Dictionary<string, object>() {
                                { Utils.ARG_SOFTWARE_SYSTEM_NAME, SoftwareSystemName },
                                { Utils.ARG_INVOCATION_INTERFACES, Utils.DbContextInterfaces },
                                { Utils.ARG_CONTAINER_NAME, "Infrastructure" }
                            }
                        }
                    }));
        }

        private void AddSoftwareSystemTypeMapping(CSharpToYamlAaCWriter writer)
        {
            var softwareSystemRootNamespace = "eShop.Catalog.API";
            var services = new Dictionary<string, string>
            {
                
            };


            foreach (var service in services)
            {
                writer.AddSoftwareSystemTypeMapping(service.Key, service.Value);
            }
        }

    }
}
