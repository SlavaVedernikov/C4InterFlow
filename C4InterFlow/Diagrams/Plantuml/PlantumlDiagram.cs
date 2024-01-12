using System.Text;
using C4InterFlow.Commons.FileSystem;

namespace C4InterFlow.Diagrams.Plantuml;

/// <summary>
/// Parser Diagram to PlantUML
/// </summary>
public static class PlantumlDiagram
{
    /// <summary>
    /// Create PUML content from Diagram
    /// </summary>
    /// <param name="diagram"></param>
    /// <returns></returns>
    public static string ToPumlString(this Diagram diagram, string path) => diagram.ToPumlString(false, path);

    /// <summary>
    /// Create PUML content from Diagram
    /// </summary>
    /// <param name="diagram"></param>
    /// <param name="useStandardLibrary"></param>
    /// <returns></returns>
    public static string ToPumlString(this Diagram diagram, bool useStandardLibrary, string diagramPath) => new StringBuilder()
            .BuildHeader(diagram, useStandardLibrary, diagramPath)
            .BuildBody(diagram)
            .BuildFooter(diagram)
            .ToString();

    private static StringBuilder BuildHeader(this StringBuilder stream, Diagram diagram, bool useStandardLibrary, string diagramPath)
    {
        var includePath = diagram.GetPumlFilePath(useStandardLibrary, diagramPath);
        stream.AppendLine($"@startuml");
        stream.AppendLine($"!include {includePath}");
        stream.AppendLine();

        BuildStyleSession(stream, diagram);

        if (diagram.LayoutWithLegend && !diagram.ShowLegend)
        {
            stream.AppendLine("LAYOUT_WITH_LEGEND()");
        }

        if (diagram.LayoutAsSketch)
        {
            stream.AppendLine("LAYOUT_AS_SKETCH()");
        }

        stream.AppendLine("SHOW_PERSON_PORTRAIT()");
        stream.AppendLine($"{(diagram.FlowVisualization == DiagramLayout.TopDown ? "LAYOUT_TOP_DOWN()" : "LAYOUT_LEFT_RIGHT()")}");
        stream.AppendLine();

        // TODO: Make this configurable via CLI diagram options
        stream.AppendLine("skinparam linetype polyline");
        stream.AppendLine();

        if (!string.IsNullOrWhiteSpace(diagram.Title))
        {
            stream.AppendLine($"title {diagram.Title}");
            stream.AppendLine();
        }

        return stream;
    }

    private static void BuildStyleSession(StringBuilder stream, Diagram diagram)
    {
        if (diagram.Tags is not null)
        {
            foreach (var (_, value) in diagram.Tags.Items)
            {
                stream.AppendLine(value);
            }

            stream.AppendLine();
        }

        if (diagram.Style is not null)
        {
            foreach (var (_, value) in diagram.Style.Items)
            {
                stream.AppendLine(value);
            }

            stream.AppendLine();
        }

        if (diagram.RelTags is not null)
        {
            foreach (var (_, value) in diagram.RelTags.Items)
            {
                stream.AppendLine(value);
            }

            stream.AppendLine();
        }
    }

    private static StringBuilder BuildBody(this StringBuilder stream, Diagram diagram)
    {
        foreach (var structure in diagram.Structures)
        {
            stream.AppendLine(structure.ToPumlString());
        }

        stream.AppendLine();

        foreach (var relationship in diagram.Relationships)
        {
            stream.AppendLine(relationship.ToPumlString());
        }

        stream.AppendLine();

        foreach (var note in diagram.Notes)
        {
            stream.AppendLine(note.ToPumlString());
        }

        return stream;
    }

    private static StringBuilder BuildFooter(this StringBuilder stream, Diagram diagram)
    {
        if (diagram.ShowLegend)
        {
            stream.AppendLine();
            stream.AppendLine("SHOW_LEGEND()");
        }

        stream.AppendLine("@enduml");

        return stream;
    }

    private static string GetPumlFilePath(this Diagram diagram, bool useUrlInclude, string path)
    {
        const string standardLibraryBaseUrl = "https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master";
        var pumlFileName = $"{diagram.Name}.puml";

        return useUrlInclude
            ? $"{standardLibraryBaseUrl}/{pumlFileName}"
            : Path.Join(
                C4Directory.GetRelativeResourcesDirectoryPath(path),
                pumlFileName);
    }
}