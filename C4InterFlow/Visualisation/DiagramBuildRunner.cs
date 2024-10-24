using C4InterFlow.Visualisation.Interfaces;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Relationships;
using C4InterFlow.Visualisation.Plantuml.Style;

namespace C4InterFlow.Visualisation;

public abstract class DiagramBuildRunner : IDiagramBuildRunner
{
    private readonly StructureCollection _structures;

    protected virtual bool LayoutWithLegend { get; }
    protected virtual bool ShowLegend { get; }
    protected virtual bool LayoutAsSketch { get; }
    protected virtual DiagramLayout FlowVisualization { get; }
    protected abstract string Title { get; }
    protected virtual string Description { get; }
    protected abstract DiagramType DiagramType { get; }
    protected abstract IEnumerable<Structure> Structures { get; }
    protected abstract IEnumerable<Relationship> Relationships { get; }
    protected virtual IEnumerable<Note> Notes { get => Array.Empty<Note>(); }
    protected virtual Flow Flow { get => new Flow(); }
    protected DiagramBuildRunner()
    {
        _structures = new StructureCollection();
        LayoutWithLegend = false;
        Description = string.Empty;
        ShowLegend = false;
        LayoutAsSketch = false;
        FlowVisualization = DiagramLayout.TopDown;
    }

    public Structure It<T>() => It(StructureIdentity.New<T>().Value);
    public Structure It<T>(int instance) => It(StructureIdentity.New<T>(instance.ToString()).Value);
    public Structure It<T>(string instance) => It(StructureIdentity.New<T>(instance).Value);

    public Structure It(string key)
        => _structures.Items[key]
           ?? throw new KeyNotFoundException($"Structure {key} not found");

    public Structure It(string key, int instance)
        => _structures.Items[new StructureIdentity(key, instance.ToString()).Value]
           ?? throw new KeyNotFoundException($"Structure {key} not found");

    public Structure It(string key, string instance)
        => _structures.Items[new StructureIdentity(key, instance).Value]
           ?? throw new KeyNotFoundException($"Structure {key} not found");

    protected virtual IElementStyle? SetStyle() => null;

    protected virtual IElementTag? SetTags()
    {
        return new ElementTag()
            .AddElementTag(Tags.LIFECYCLE_NEW, bgColor: "green", borderColor: "green")
            .AddElementTag(Tags.LIFECYCLE_CHANGED, bgColor: "orange", borderColor: "orange")
            .AddElementTag(Tags.LIFECYCLE_REMOVED, bgColor: "red", borderColor: "red");
    }

    protected virtual IRelationshipTag? SetRelTags()
    {
        return new RelationshipTag()
            .AddRelTag(Tags.LIFECYCLE_NEW, "green", "green")
            .AddRelTag(Tags.LIFECYCLE_CHANGED, "orange", "orange")
            .AddRelTag(Tags.LIFECYCLE_REMOVED, "red", "red");
    }

    public Diagram Build()
    {
        _structures.AddRange(Structures);

        return new Diagram(DiagramType) with
        {
            Structures = Structures,
            Relationships = Relationships,
            Notes = Notes.ToArray(),
            Flow = Flow,
            Title = Title,
            ShowLegend = ShowLegend,
            Description = Description,
            LayoutWithLegend = LayoutWithLegend,
            LayoutAsSketch = LayoutAsSketch,
            FlowVisualization = FlowVisualization,
            Tags = SetTags(),
            RelTags = SetRelTags(),
            Style = SetStyle()
        };
    }

    protected virtual IList<Structure> CleanUpStructures(IList<Structure> structures)
    {
        foreach (var item in structures.Where(x => x.Alias == SoftwareSystems.ExternalSystem.ALIAS).ToArray())
        {
            structures.Remove(item);
        }

        return structures;
    }
    protected virtual IList<Relationship> CleanUpRelationships(IList<Relationship> relationships, int maxLineLabels, bool isStatic)
    {
        if (relationships == null) return null;

        List<Relationship> relationshipsToRemove = new List<Relationship>();
        List<Relationship> relationshipsToAdd = new List<Relationship>();

        var relationshipGroups = relationships.GroupBy(r => new
        {
            r.From,
            r.To
        })
        .Select(g => new
        {
            g.Key,
            Relationships = g.ToList()
        });


        foreach (var duplicateRelationshipGroup in relationshipGroups)
        {
            relationshipsToRemove.AddRange(duplicateRelationshipGroup.Relationships);

            var fromStructure = default(Structure);
            try
            {
                fromStructure = It(duplicateRelationshipGroup.Key.From);
            }
            catch { }

            var toStructure = default(Structure);
            try
            {
                toStructure = It(duplicateRelationshipGroup.Key.To);
            }
            catch { }

            if (fromStructure != default(Structure) && toStructure != default(Structure))
            {
                var relationshipProtocolGroups = duplicateRelationshipGroup.Relationships.GroupBy(r => new
                {
                    r.Protocol
                }).Select(g => new
                {
                    g.Key,
                    Relationships = g.ToList()
                });

                foreach (var relationshipProtocolGroup in relationshipProtocolGroups)
                {
                    var protocol = relationshipProtocolGroup.Key.Protocol;

                    var labels = relationshipProtocolGroup.Relationships.Select(x => $"{x.Label}").Distinct();

                    if(labels.Count() > maxLineLabels)
                    {
                        labels = labels.Take(maxLineLabels).Append("***");
                    }

                    var label = isStatic ? $"Uses" : string.Join("\\n", labels);

                    var relationship = (fromStructure > toStructure)[label, protocol];

                    if(!string.IsNullOrEmpty(protocol))
                    {
                        relationship = relationship.AddTags($"\"{nameof(Relationship.Protocol).ToLower()}:{protocol.ToLower()}\"");
                    }
                    
                    relationshipsToAdd.Add(relationship);
                }
            }
                
        }


        foreach (var item in relationshipsToRemove)
        {
            relationships.Remove(item);
        }

        foreach (var item in relationshipsToAdd)
        {
            relationships.Add(item);
        }

        foreach (var item in relationships.Where(x => x.From == x.To).ToArray())
        {
            relationships.Remove(item);
        }

        foreach (var item in relationships.Where(x => x.From == SoftwareSystems.ExternalSystem.ALIAS).ToArray())
        {
            relationships.Remove(item);
        }

        return relationships;
    }
}