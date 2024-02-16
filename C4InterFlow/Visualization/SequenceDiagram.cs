using C4InterFlow.Visualization.Plantuml.Style;
using C4InterFlow.Visualization.Interfaces;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Boundaries;
using C4InterFlow.Structures.Relationships;
using C4InterFlow.Commons.Extensions;

namespace C4InterFlow.Visualization
{

    public class SequenceDiagram : DiagramBuildRunner
    {
        public SequenceDiagram(string title, BusinessProcess process)
        {
            DiagramTitle = title;
            var interfaces = new List<Interface>();
            foreach (var activity in process.Activities)
            {
                interfaces.AddRange(activity.Flow.GetUseFlows().Select(x => Utils.GetInstance<Interface>(x.Expression)));
            }
            Interfaces = interfaces;
        }

        private IEnumerable<Interface> Interfaces { get; }
        private string DiagramTitle { get; }
        protected override string Title => DiagramTitle;
        protected override DiagramType DiagramType => DiagramType.Component;

        protected override bool ShowLegend => true;

        private List<Structure> _structures;
        protected override IEnumerable<Structure> Structures
        {
            get
            {
                {
                    if (_structures == null)
                    {
                        _structures = new List<Structure>
                        {
                            SoftwareSystems.ExternalSystem.Interfaces.ExternalInterface.Instance
                        };
                        foreach (var item in Interfaces)
                        {
                            GetStructures(_structures, item);
                        }
                    }

                    return _structures;
                }
            }
        }

        private IList<Structure> GetStructures(IList<Structure> structures, Interface @interface)
        {
            var interfaceOwner = Utils.GetInstance<Structure>(@interface.Owner);
            if (!structures.Any(x => x.Alias == interfaceOwner.Alias))
            {
                if (interfaceOwner is Container || interfaceOwner is SoftwareSystem)
                {
                    structures.Add(interfaceOwner);
                }
                else if (interfaceOwner is Component component)
                {
                    var container = Utils.GetInstance<Container>(component.Container);
                    var containerBoundary = structures.OfType<ContainerBoundary>().FirstOrDefault(x => x.Alias == container.Alias);

                    if (containerBoundary == null)
                    {
                        containerBoundary = new ContainerBoundary(container.Alias, $"{container.Label}")
                        {
                            Components = new List<Component>(),
                        };
                        structures.Add(containerBoundary);
                    }
                    if (!containerBoundary.Components.Any(x => x.Alias == interfaceOwner.Alias))
                    {
                        ((List<Component>)containerBoundary.Components).Add(component);
                    }
                }
            }

            foreach (var usesInterface in @interface.Flow.GetUsesInterfaces())
            {
                GetStructures(structures, usesInterface);
            }

            return structures;
        }

        private List<Relationship> _relationships;
        protected override IEnumerable<Relationship> Relationships
        {
            get
            {
                {
                    if (_relationships == null)
                    {
                        _relationships = new List<Relationship>();

                        foreach (var item in Interfaces)
                        {
                            GetRelationships(_relationships, SoftwareSystems.ExternalSystem.Interfaces.ExternalInterface.Instance, item);
                        }

                    }

                    return _relationships;
                }
            }
        }

        private IList<Relationship> GetRelationships(IList<Relationship> relationships, Interface @interface, Interface usesInterface)
        {
            var interfaceOwner = Utils.GetInstance<Structure>(@interface.Owner);
            var usesInterfaceOwner = Utils.GetInstance<Structure>(usesInterface.Owner);

            if (!relationships.Any(x => x.From == (interfaceOwner ?? @interface).Alias &&
                                        x.To == usesInterfaceOwner.Alias &&
                                        x.Label == usesInterface.Label))
            {
                var input = Utils.GetInstance<Structure>(usesInterface.Input);
                var output = Utils.GetInstance<Structure>(usesInterface.Output);
                var outputTemplate = Utils.GetInstance<Structure>(usesInterface.OutputTemplate);

                var inputLable = input != null ? $"{input.Label} -> " : string.Empty;
                var outputLable = $"{(outputTemplate != null || output != null ? " -> " : string.Empty)}{(outputTemplate != null ? $"{outputTemplate.Label}<" : string.Empty)}{(output != null ? output.Label : string.Empty)}{(outputTemplate != null ? ">" : string.Empty)}";

                relationships.Add(((interfaceOwner ?? @interface) > usesInterfaceOwner)[
                    $"{inputLable}{usesInterface.Label}{outputLable}",
                    usesInterface.Protocol].AddTags(usesInterface.Tags?.ToArray()));
            }


            foreach (var usesAnotherInterface in usesInterface.Flow.GetUsesInterfaces())
            {
                GetRelationships(relationships, usesInterface, usesAnotherInterface);
            }

            return relationships;
        }

        protected override IElementTag? SetTags()
        {
            return new ElementTag()
                .AddElementTag(Tags.STATE_NEW, bgColor: "green", borderColor: "green")
                .AddElementTag(Tags.STATE_CHANGED, bgColor: "orange", borderColor: "orange")
                .AddElementTag(Tags.STATE_REMOVED, bgColor: "red", borderColor: "red");
        }

        protected override IRelationshipTag? SetRelTags()
        {
            return new RelationshipTag()
                .AddRelTag(Tags.STATE_NEW, "green", "green")
                .AddRelTag(Tags.STATE_CHANGED, "orange", "orange")
                .AddRelTag(Tags.STATE_REMOVED, "red", "red");
        }
    }
}