using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using C4InterFlow.Structures;
using C4InterFlow.Automation.Readers;
using System.Text.RegularExpressions;
using C4InterFlow.Structures.Boundaries;
using System;
using YamlDotNet.Core.Tokens;
using C4InterFlow.Structures.Interfaces;

namespace C4InterFlow.Automation.Writers
{
    public class YamlToCsvAaCWriter : YamlToAnyAaCWriter
    {
        private string FileExtension => "csv";
        private CsvDataProvider DataProvider { get; init; }
        public YamlToCsvAaCWriter(string architectureInputPath, string architectureOutputPath) : base(architectureInputPath)
        {
            DataProvider = new CsvDataProvider(architectureOutputPath, CsvDataProvider.Mode.Write);
        }

        public override IAaCWriter AddActor(string name, string type, string? label = null)
        {
            DataProvider.ActorRecords.Add(new CsvDataProvider.Actor()
            {
                Alias = name,
                TypeName = type,
                Name = label ?? AnyCodeWriter.GetLabel(name) ?? string.Empty
            });
            return this;
        }

        public override IAaCWriter AddBusinessProcess(string name, string? label = null)
        {
            DataProvider.BusinessProcessRecords.Add(new CsvDataProvider.BusinessProcess()
            {
                Alias = name,
                Name = label ?? AnyCodeWriter.GetLabel(name) ?? string.Empty
            });
            return this;
        }

        public IAaCWriter AddActivity(string businessProcessName, string actor, IEnumerable<Flow> flows, string? label = null)
        {
            if (flows == null) return this;

            foreach (var flow in flows)
            {
                DataProvider.ActivityRecords.Add(new CsvDataProvider.Activity()
                {
                    BusinessProcess = businessProcessName,
                    Name = label ?? string.Empty,
                    Actor = actor,
                    UsesContainerInterface = (flow.Expression != null && Regex.IsMatch(flow.Expression, AnyCodeWriter.ContainerInterfacePattern) ?
                    flow.Expression.Replace($"{ArchitectureNamespace}.SoftwareSystems.", string.Empty) : string.Empty) ?? string.Empty,
                    UsesSoftwareSystemInterface = (flow.Expression != null && Regex.IsMatch(flow.Expression, AnyCodeWriter.SoftwareSystemInterfacePattern) ?
                    flow.Expression.Replace($"{ArchitectureNamespace}.SoftwareSystems.", string.Empty) : string.Empty) ?? string.Empty
                });
            }
                
            return this;
        }

        public override IAaCWriter AddSoftwareSystem(string name, string? boundary = null, string? label = null, string? description = null)
        {
            DataProvider.SoftwareSystemRecords.Add(new CsvDataProvider.SoftwareSystem()
            {
                Alias = name,
                Name = label ?? AnyCodeWriter.GetLabel(name) ?? string.Empty,
                IsExternal = Enum.TryParse<Boundary>(boundary, out var boundaryValue) ? boundaryValue == Boundary.External : false,
                Description = description
            });
            return this;
        }

        public override IAaCWriter AddSoftwareSystemInterface(
            string softwareSystemName,
            string name,
            string? label = null,
            string? description = null,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null)
        {
            DataProvider.SoftwareSystemInterfaceRecords.Add(new CsvDataProvider.SoftwareSystemInterface()
            {
                SoftwareSystem = softwareSystemName,
                Alias = $"{softwareSystemName}.Interfaces.{name}",
                Name = label ?? AnyCodeWriter.GetLabel(name) ?? string.Empty,
                Description = description ?? string.Empty,
                Protocol = protocol ?? string.Empty,
            });
            return this;
        }

        public YamlToCsvAaCWriter AddSoftwareSystemInterfaceFlows(Interface interfaceInstance)
        {
            foreach (var flow in interfaceInstance.Flow?.GetUseFlows() ?? Enumerable.Empty<Flow>())
            {
                DataProvider.SoftwareSystemInterfaceFlowRecords.Add(new CsvDataProvider.SoftwareSystemInterfaceFlow()
                {
                    SoftwareSystemInterface = interfaceInstance.Alias.Replace($"{ArchitectureNamespace}.SoftwareSystems.", string.Empty),
                    UsesContainerInterface = (flow.Expression != null && Regex.IsMatch(flow.Expression, AnyCodeWriter.ContainerInterfacePattern) ? 
                    flow.Expression.Replace($"{ArchitectureNamespace}.SoftwareSystems.", string.Empty) : string.Empty) ?? string.Empty,
                    UsesSoftwareSystemInterface = (flow.Expression != null && Regex.IsMatch(flow.Expression, AnyCodeWriter.SoftwareSystemInterfacePattern) ? 
                    flow.Expression.Replace($"{ArchitectureNamespace}.SoftwareSystems.", string.Empty) : string.Empty) ?? string.Empty
                });
            }
            return this;
        }

        public override IAaCWriter AddContainer(string softwareSystemName, string name, string? containerType = null, string? label = null, string? description = null)
        {
            DataProvider.ContainerRecords.Add(new CsvDataProvider.Container()
            {
                Alias = $"{softwareSystemName}.Containers.{name}",
                SoftwareSystem = softwareSystemName,
                Name = label ?? AnyCodeWriter.GetLabel(name) ?? string.Empty,
                Description = description ?? string.Empty,
                Type = Enum.TryParse<ContainerType>((containerType ?? string.Empty), out var containerTypeValue) ?
                Enum.GetName(typeof(ContainerType), containerTypeValue)!:
                Enum.GetName(typeof(ContainerType), ContainerType.None)!
            });
            return this;
        }

        public override IAaCWriter AddContainerInterface(
            string softwareSystemName,
            string containerName,
            string name,
            string? label = null,
            string? description = null,
            string? input = null,
            string? output = null,
            string? protocol = null,
            string? path = null)
        {
            DataProvider.ContainerInterfaceRecords.Add(new CsvDataProvider.ContainerInterface()
            {
                Alias = $"{softwareSystemName}.Containers.{containerName}.Interfaces.{name}",
                Container = $"{softwareSystemName}.Containers.{containerName}",
                Name = label ?? AnyCodeWriter.GetLabel(name) ?? string.Empty,
                Description = description ?? string.Empty,
                Protocol = protocol ?? string.Empty
            });
            return this;
        }

        public YamlToCsvAaCWriter AddContainerInterfaceFlows(Interface interfaceInstance)
        {
            foreach (var flow in interfaceInstance.Flow?.GetUseFlows() ?? Enumerable.Empty<Flow>())
            {
                DataProvider.ContainerInterfaceFlowRecords.Add(new CsvDataProvider.ContainerInterfaceFlow()
                {
                    ContainerInterface = interfaceInstance.Alias.Replace($"{ArchitectureNamespace}.SoftwareSystems.", string.Empty),
                    UsesContainerInterface = (flow.Expression != null && Regex.IsMatch(flow.Expression, AnyCodeWriter.ContainerInterfacePattern) ? 
                    flow.Expression?.Replace($"{ArchitectureNamespace}.SoftwareSystems.", string.Empty) : string.Empty) ?? string.Empty,
                    UsesSoftwareSystemInterface = (flow.Expression != null && Regex.IsMatch(flow.Expression, AnyCodeWriter.SoftwareSystemInterfacePattern) ? 
                    flow.Expression.Replace($"{ArchitectureNamespace}.SoftwareSystems.", string.Empty) : string.Empty) ?? string.Empty,
                });
            }
            return this;
        }

        public override string GetFileExtension()
        {
            return FileExtension;
        }

        public void WriteArchitecture()
        {
            DataProvider.WriteData();
        }
    }
}
