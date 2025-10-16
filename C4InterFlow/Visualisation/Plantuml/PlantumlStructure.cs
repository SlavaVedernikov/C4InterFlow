using C4InterFlow.Commons;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Boundaries;
using C4InterFlow.Structures.Interfaces;
using C4InterFlow.Visualisation.Plantuml.Enums;
using System.Text;

namespace C4InterFlow.Visualisation.Plantuml;

/// <summary>
/// PlantUML Parser
/// </summary>
internal static class PlantumlStructure
{
    /// <summary>
    /// Parser Structure to PlantUML
    /// </summary>
    public static string ToPumlString(this Structure structure, 
        BoundaryStyle? style = BoundaryStyle.CurlyBracketsEnclosed,
        bool ignoreTags = false)
        => structure switch
        {
            Person person => person.ToPumlString(ignoreTags),
            SoftwareSystem system => system.ToPumlString(ignoreTags),
            SoftwareSystemBoundary softwareSystemBoundary => softwareSystemBoundary.ToPumlString(style.Value, ignoreTags),
            DeploymentNode deploymentNode => deploymentNode.ToPumlString(concat:0, ignoreTags),
            Component component => component.ToPumlString(ignoreTags),
            Container container => container.ToPumlString(ignoreTags),
            ContainerBoundary containerBoundary => containerBoundary.ToPumlString(style, ignoreTags),
            EnterpriseBoundary enterpriseBoundary => enterpriseBoundary.ToPumlString(style, ignoreTags),
            _ => string.Empty
        };

    private static string ToPumlString(this Person person, bool ignoreTags = false)
    {
        var procedureName = $"Person{GetExternalSuffix(person)}";

        return
            $"{procedureName}({person.Alias}, \"{person.Label}\", \"{person.Description}\""
                .TryConcatTags(person, ignoreTags) + ")";
    }

    private static string ToPumlString(this SoftwareSystem system, bool ignoreTags = false)
    {
        var procedureName = $"System{GetExternalSuffix(system)}";

        return
            $"{procedureName}({system.Alias}, \"{system.Label}\", \"{system.Description}\""
                .AddIcon(system).TryConcatTags(system, ignoreTags) + ")";
    }

    private static string ToPumlString(this SoftwareSystemBoundary boundary, BoundaryStyle? style = BoundaryStyle.CurlyBracketsEnclosed, bool ignoreTags = false)
    {
        var stream = new StringBuilder();
        stream.AppendLine();
        stream.AppendLine($"System_Boundary({boundary.Alias}, \"{boundary.Label}\"){(style!.Value == BoundaryStyle.CurlyBracketsEnclosed ? " {" : string.Empty)}");

        foreach (var structure in boundary.Structures)
        {
            stream.AppendLine($"{TabIndentation.Indent()}{structure.ToPumlString(style, ignoreTags)}");
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

    private static string ToPumlString(this EnterpriseBoundary boundary, BoundaryStyle? style = BoundaryStyle.CurlyBracketsEnclosed, bool ignoreTags = false)
    {
        var stream = new StringBuilder();
        stream.AppendLine();
        stream.AppendLine($"Enterprise_Boundary({boundary.Alias}, \"{boundary.Label}\"){(style!.Value == BoundaryStyle.CurlyBracketsEnclosed ? " {" : string.Empty)}");

        foreach (var structure in boundary.Structures)
        {
            if (structure is Person or SoftwareSystem or EnterpriseBoundary)
            {
                stream.AppendLine($"{TabIndentation.Indent()}{structure.ToPumlString(style, ignoreTags)}");
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

    private static string ToPumlString(this Component component, bool ignoreTags = false)
    {
        var externalSuffix = GetExternalSuffix(component);
        var procedureName = component.ComponentType switch
        {
            ComponentType.Database => $"ComponentDb{externalSuffix}",
            ComponentType.Queue => $"ComponentQueue{externalSuffix}",
            ComponentType.Topic => $"ComponentQueue{externalSuffix}",
            _ => $"Component{externalSuffix}"
        };

        var technology = string.Join(", ", new[] { GetDescription(component.ComponentType), component.Technology }.Where(s => !string.IsNullOrWhiteSpace(s)));

        return
            $"{procedureName}({component.Alias}, \"{component.Label}\", \"{technology}\", \"{component.Description}\""
                .AddIcon(component).TryConcatTags(component, ignoreTags) + ")";
    }

    private static string ToPumlString(this Container container, bool ignoreTags = false)
    {
        var externalSuffix = GetExternalSuffix(container);

        var procedureName = container.ContainerType switch
        {
            ContainerType.Database => $"ContainerDb{externalSuffix}",
            ContainerType.Queue => $"ContainerQueue{externalSuffix}",
            ContainerType.Topic => $"ContainerQueue{externalSuffix}",
            _ => $"Container{externalSuffix}"
        };

        var technology = string.Join(", ", new[] { GetDescription(container.ContainerType), container.Technology }.Where(s => !string.IsNullOrWhiteSpace(s)));

        return $"{procedureName}({container.Alias}, \"{container.Label}\", \"{technology}\", \"{container.Description}\""
            .AddIcon(container).TryConcatTags(container, ignoreTags) + ")";
    }

    private static string ToPumlString(this ContainerBoundary boundary, BoundaryStyle? style = BoundaryStyle.CurlyBracketsEnclosed, bool ignoreTags = false)
    {
        var stream = new StringBuilder();

        stream.AppendLine();
        stream.AppendLine($"Container_Boundary({boundary.Alias}, \"{boundary.Label}\"){(style!.Value == BoundaryStyle.CurlyBracketsEnclosed ? " {" : string.Empty)}");

        foreach (var component in boundary.Components)
        {
            stream.AppendLine($"{TabIndentation.Indent()}{component.ToPumlString(ignoreTags)}");
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

    private static string ToPumlString(this DeploymentNode deployment, int concat = 0, bool ignoreTags = false)
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

        stream.AppendLine($"{spaces}Deployment_Node({deployment.Alias}, \"{deployment.Label}\", \"{deployment.Description}\"".TryConcatTags(deployment, ignoreTags) + ") {");

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

    private static string GetExternalSuffix(ISupportBoundary structure) =>
        structure.Boundary == Boundary.External ? "_Ext" : string.Empty;

    private static string GetDescription<TEnum>(TEnum enumValue) where TEnum : struct, Enum
    {
        var field = typeof(TEnum).GetField(enumValue.ToString());
        var descriptionAttribute = field?.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
            .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
        return descriptionAttribute?.Description ?? enumValue.ToString();
    }

    private static string TryConcatTags(this string value, Structure structure, bool ignoreTags = false) =>
         value + (!ignoreTags && structure.Tags.Any() ? $", $tags=\"{string.Join("+", structure.Tags)}\"" : string.Empty);

    private static string AddIcon(this string value, string icon) =>
        value + (!string.IsNullOrEmpty(icon) ? $", \"{icon.Split('/').Last()}\"" : string.Empty);
    private static string AddIcon(this string value, SoftwareSystem system) =>
         value.AddIcon(system.Icon);

    private static string AddIcon(this string value, Container container) =>
         value.AddIcon(container.Icon);

    private static string AddIcon(this string value, Component component) =>
         value.AddIcon(component.Icon);
}
