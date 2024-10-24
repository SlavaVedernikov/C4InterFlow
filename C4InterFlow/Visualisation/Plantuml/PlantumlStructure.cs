using System.Text;
using C4InterFlow.Commons;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Boundaries;
using C4InterFlow.Visualisation.Plantuml.Enums;

namespace C4InterFlow.Visualisation.Plantuml;

/// <summary>
/// PlantUML Parser
/// </summary>
internal static class PlantumlStructure
{
    /// <summary>
    /// Parser Structure to PlantUML
    /// </summary>
    public static string ToPumlString(this Structure structure, BoundaryStyle? style = BoundaryStyle.CurlyBracketsEnclosed)
        => structure switch
        {
            Person person => person.ToPumlString(),
            SoftwareSystem system => system.ToPumlString(),
            SoftwareSystemBoundary softwareSystemBoundary => softwareSystemBoundary.ToPumlString(style),
            DeploymentNode deploymentNode => deploymentNode.ToPumlString(),
            Component component => component.ToPumlString(),
            Container container => container.ToPumlString(),
            ContainerBoundary containerBoundary => containerBoundary.ToPumlString(style),
            EnterpriseBoundary enterpriseBoundary => enterpriseBoundary.ToPumlString(style),
            _ => string.Empty
        };

    private static string ToPumlString(this Person person)
    {
        var procedureName = $"Person{GetExternalSuffix(person)}";

        return
            $"{procedureName}({person.Alias}, \"{person.Label}\", \"{person.Description}\""
                .TryConcatTags(person) + ")";
    }

    private static string ToPumlString(this SoftwareSystem system)
    {
        var procedureName = $"System{GetExternalSuffix(system)}";

        return
            $"{procedureName}({system.Alias}, \"{system.Label}\", \"{system.Description}\""
                .AddIcon(system).TryConcatTags(system) + ")";
    }

    private static string ToPumlString(this SoftwareSystemBoundary boundary, BoundaryStyle? style = BoundaryStyle.CurlyBracketsEnclosed)
    {
        var stream = new StringBuilder();
        stream.AppendLine();
        stream.AppendLine($"System_Boundary({boundary.Alias}, \"{boundary.Label}\"){(style!.Value == BoundaryStyle.CurlyBracketsEnclosed ? " {" : string.Empty)}");

        foreach (var structure in boundary.Structures)
        {
            stream.AppendLine($"{TabIndentation.Indent()}{structure.ToPumlString(style)}");
        }

        if (style!.Value == BoundaryStyle.CurlyBracketsEnclosed)
        {
            stream.Append("}");
        }
        else
        {
            stream.Append("Boundary_End()");
        }
        

        return stream.ToString();
    }

    private static string ToPumlString(this EnterpriseBoundary boundary, BoundaryStyle? style = BoundaryStyle.CurlyBracketsEnclosed)
    {
        var stream = new StringBuilder();
        stream.AppendLine();
        stream.AppendLine($"Enterprise_Boundary({boundary.Alias}, \"{boundary.Label}\"){(style!.Value == BoundaryStyle.CurlyBracketsEnclosed ? " {" : string.Empty)}");

        foreach (var structure in boundary.Structures)
        {
            if (structure is Person or SoftwareSystem or EnterpriseBoundary)
            {
                stream.AppendLine($"{TabIndentation.Indent()}{structure.ToPumlString(style)}");
            }
        }

        if (style!.Value == BoundaryStyle.CurlyBracketsEnclosed)
        {
            stream.Append("}");
        }
        else
        {
            stream.Append("Boundary_End()");
        }

        return stream.ToString();
    }

    private static string ToPumlString(this Component component)
    {
        var externalSuffix = GetExternalSuffix(component);
        var procedureName = component.ComponentType switch
        {
            ComponentType.Database => $"ComponentDb{externalSuffix}",
            ComponentType.Queue => $"ComponentQueue{externalSuffix}",
            _ => $"Component{externalSuffix}"
        };

        return
            $"{procedureName}({component.Alias}, \"{component.Label}\", \"{component.Technology}\", \"{component.Description}\""
                .AddIcon(component).TryConcatTags(component) + ")";
    }

    private static string ToPumlString(this Container container)
    {
        var externalSuffix = GetExternalSuffix(container);

        var procedureName = container.ContainerType switch
        {
            ContainerType.Database => $"ContainerDb{externalSuffix}",
            ContainerType.Queue => $"ContainerQueue{externalSuffix}",
            ContainerType.Topic => $"ContainerQueue{externalSuffix}",
            _ => $"Container{externalSuffix}"
        };

        return $"{procedureName}({container.Alias}, \"{container.Label}\", \"{container.Technology}\", \"{container.Description}\""
            .AddIcon(container).TryConcatTags(container) + ")";
    }

    private static string ToPumlString(this ContainerBoundary boundary, BoundaryStyle? style = BoundaryStyle.CurlyBracketsEnclosed)
    {
        var stream = new StringBuilder();

        stream.AppendLine();
        stream.AppendLine($"Container_Boundary({boundary.Alias}, \"{boundary.Label}\"){(style!.Value == BoundaryStyle.CurlyBracketsEnclosed ? " {" : string.Empty)}");

        foreach (var component in boundary.Components)
        {
            stream.AppendLine($"{TabIndentation.Indent()}{component.ToPumlString()}");
        }

        if (style!.Value == BoundaryStyle.CurlyBracketsEnclosed)
        {
            stream.Append("}");
        }
        else
        {
            stream.Append("Boundary_End()");
        }

        return stream.ToString();
    }

    private static string ToPumlString(this DeploymentNode deployment, int concat = 0)
    {
        var stream = new StringBuilder();
        var spaces = TabIndentation.Indent(concat);

        if (concat == 0)
        {
            stream.AppendLine();
        }

        foreach (var (key, value) in deployment.Properties)
        {
            stream.AppendLine($"{spaces}AddProperty(\"{key}\", \"{value}\")");
        }

        stream.AppendLine($"{spaces}Deployment_Node({deployment.Alias}, \"{deployment.Label}\", \"{deployment.Description}\"".TryConcatTags(deployment) + ") {");

        foreach (var node in deployment.Nodes)
        {
            stream.AppendLine($"{node.ToPumlString(concat + TabIndentation.TabSize)}");
        }

        if (deployment.Container != null)
        {
            stream.AppendLine(TabIndentation.Indent(concat) + deployment.Container.ToPumlString());
        }

        stream.Append(spaces + "}");

        return stream.ToString();
    }

    private static string GetExternalSuffix(Structure structure) =>
        structure.Boundary == Boundary.External ? "_Ext" : string.Empty;

    private static string TryConcatTags(this string value, Structure structure) =>
         value + (structure.Tags.Any() ? $", $tags=\"{string.Join("+", structure.Tags)}\"" : string.Empty);

    private static string AddIcon(this string value, string icon) =>
        value + (!string.IsNullOrEmpty(icon) ? $", \"{icon.Split('/').Last()}\"" : string.Empty);
    private static string AddIcon(this string value, SoftwareSystem system) =>
         value.AddIcon(system.Icon);

    private static string AddIcon(this string value, Container container) =>
         value.AddIcon(container.Icon);

    private static string AddIcon(this string value, Component component) =>
         value.AddIcon(component.Icon);
}
