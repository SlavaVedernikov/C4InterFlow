using C4InterFlow.Attributes;
using C4InterFlow.Visualisation.Interfaces;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Boundaries;
using C4InterFlow.Structures.Relationships;
using C4InterFlow.Visualisation.Plantuml.Style;
using C4InterFlow.Cli.Commands.Options;

namespace C4InterFlow.Visualisation
{
    public class ComponentDiagram : DiagramBuildRunner
    {
        private Note[]? _notes = null;

        public ComponentDiagram (
            string title, 
            BusinessProcess process, 
            bool showBoundaries = false, 
            bool showInterfaceInputAndOutput = false, 
            int maxLineLabels = DiagramMaxLineLabelsOption.DefaultValue, 
            bool isStatic = false, 
            Note[]? notes = null)
        {
            DiagramTitle = title;
            Process = process;
            ShowBoundaries = showBoundaries;
            ShowInterfaceInputAndOutput = showInterfaceInputAndOutput;
            MaxLineLabels = maxLineLabels;
            IsStatic = isStatic;
            _notes = notes;
        }

        private bool ShowBoundaries { get; init; }
        private int MaxLineLabels { get; init; }
        private bool IsStatic { get; init; }
        private bool ShowInterfaceInputAndOutput { get; init; }
        private BusinessProcess Process { get; init; }

        private string DiagramTitle { get; }
        protected override string Title => DiagramTitle;
        protected override DiagramType DiagramType => IsStatic ? DiagramType.ComponentStatic : DiagramType.Component;

        protected override bool ShowLegend => true;

        private List<Note> _allNotes;
        protected override IEnumerable<Note> Notes
        {
            get
            {
                if (_allNotes == null)
                {
                    _allNotes = new List<Note>();

                    _allNotes.AddRange(_notes ?? Array.Empty<Note>());

                    foreach (var structure in Structures)
                    {
                        if (structure is ContainerBoundary)
                        {
                            var containerBoundary = structure as ContainerBoundary;
                            if (containerBoundary?.Components != null)
                            {
                                foreach (var component in containerBoundary.Components)
                                {
                                    _allNotes.AddRange((component as Component)?.Notes ?? Array.Empty<Note>());
                                }
                            }

                        }
                        _allNotes.AddRange((structure as Component)?.Notes ?? Array.Empty<Note>());
                    }
                }

                return _allNotes;
            }
        }

        private Flow _flow;
        protected override Flow Flow
        {
            get
            {
                if (_flow == null)
                {
                    _flow = new Flow();

                    foreach (var activity in Process.Activities)
                    {
                        var parentFlow = _flow;

                        if(Process.Activities.Count() > 1)
                        {
                            var actor = activity.GetActorInstance() ?? new SoftwareSystems.ExternalSystem.Interfaces.ExternalInterface().Instance;
                            //TODO: Consider refactoring this so that it is treated as a divider/separator e.g. "== {actor.Label} =="
                            parentFlow = _flow.Group(
                                $"{actor.Label}{(!string.IsNullOrEmpty(activity.Label) ? $" - {activity.Label}" : string.Empty)}",
                                actor.Alias);
                        }
                        

                        var activityFlow = Utils.Clone(activity.Flow);
                        foreach (var useFlow in activityFlow?.GetUseFlows() ?? Enumerable.Empty<Flow>())
                        {
                            PopulateFlow(useFlow);
                        }

                        parentFlow.AddFlowsRange(activityFlow.Flows);

                        if(parentFlow.Type == Flow.FlowType.Group)
                        {
                            parentFlow.EndGroup();
                        }
                    }
                }

                return _flow;
            }
        }

        private void PopulateFlow(Flow flow)
        {
            var usesInterface = Utils.GetInstance<Interface>(flow.Expression);
            var usesInterfaceOwner = Utils.GetInstance<Structure>(usesInterface?.Owner);

            if (usesInterface == null || usesInterfaceOwner == null) return;

            var currentFlow = Utils.Clone(usesInterface.Flow);
            foreach (var useFlow in currentFlow?.GetUseFlows() ?? Enumerable.Empty<Flow>())
            {
                PopulateFlow(useFlow);
            }

            flow.AddFlowsRange(currentFlow?.Flows ?? Enumerable.Empty<Flow>());
        }

        private List<Structure> _structures;
        protected override IEnumerable<Structure> Structures
        {
            get
            {
                if (_structures == null)
                {
                    _structures = new List<Structure>();

                    foreach (var activity in Process.Activities)
                    {
                        var actor = activity.GetActorInstance();
                        if (actor != null && !_structures.Any(i => i.Alias == actor.Alias))
                        {
                            _structures.Add(actor);
                        }

                        foreach (var @interface in activity.Flow?.GetUsesInterfaces() ?? Enumerable.Empty<Interface>())
                        {
                            PopulateStructures(_structures, @interface);
                        }
                    }

                    _structures = CleanUpStructures(_structures).ToList();
                }

                return _structures;
            }
        }

        //TODO: Review and test this logic
        protected override IList<Structure> CleanUpStructures(IList<Structure> structures)
        {
            structures = base.CleanUpStructures(structures);

            if (ShowBoundaries)
            {
                var emptyContainerBoundaries = structures.Where(x => x is ContainerBoundary && ((ContainerBoundary)x).Components.Count() == 0).ToList();

                foreach (var item in emptyContainerBoundaries)
                {
                    structures.Remove(item);

                    var structure = Utils.GetInstance<Structure>(item.Alias);
                    if (structure != null)
                    {
                        structures.Add(structure);
                    }
                }
            }

            return structures;
        }
        
        private void PopulateStructures(IList<Structure> structures, Interface @interface)
        {
            var interfaceOwner = Utils.GetInstance<Structure>(@interface.Owner);

            if (interfaceOwner == null) return;

            if (interfaceOwner is SoftwareSystem &&
                !structures.OfType<SoftwareSystem>().Any(x => x.Alias == interfaceOwner.Alias))
            {
                structures.Add(interfaceOwner);
            }
            else if (interfaceOwner is Container)
            {
                if (ShowBoundaries)
                {
                    var container = (Container)interfaceOwner;
                    var softwareSystem = Utils.GetInstance<SoftwareSystem>(container.SoftwareSystem);
                    var softwareSystemBoundary = structures.OfType<SoftwareSystemBoundary>().FirstOrDefault(x => x.Alias == softwareSystem.Alias);

                    if (softwareSystemBoundary == null)
                    {
                        softwareSystemBoundary = new SoftwareSystemBoundary(softwareSystem.Alias, softwareSystem.Label)
                        {
                            Structures = new List<Structure>()
                        };

                        structures.Add(softwareSystemBoundary);
                    }

                    if (!softwareSystemBoundary.Structures.Any(x => x.Alias == container.Alias))
                    {
                        ((List<Structure>)softwareSystemBoundary.Structures).Add(container);
                    }
                }
                else
                {
                    if(!structures.OfType<Container>().Any(x => x.Alias == interfaceOwner.Alias))
                    {
                        structures.Add(interfaceOwner);
                    }
                }
            }
            else if (interfaceOwner is Component)
            {
                if (ShowBoundaries)
                {
                    var container = Utils.GetInstance<Container>(((Component)interfaceOwner).Container);
                    var softwareSystem = Utils.GetInstance<SoftwareSystem>(container.SoftwareSystem);
                    var softwareSystemBoundary = structures.OfType<SoftwareSystemBoundary>().FirstOrDefault(x => x.Alias == softwareSystem.Alias);
                    var containerBoundary = softwareSystemBoundary?.Structures.OfType<ContainerBoundary>().FirstOrDefault(x => x.Alias == container.Alias);

                    if (containerBoundary == null)
                    {
                        containerBoundary = new ContainerBoundary(container.Alias, $"{container.Label}")
                        {
                            Components = new List<Component>(),
                            //TODO: Review Tags usage
                            //Tags = container.Tags
                        };
                    }

                    if (!containerBoundary.Components.Any(x => x.Alias == interfaceOwner.Alias))
                    {
                        ((List<Component>)containerBoundary.Components).Add(interfaceOwner as Component);
                    }

                    if(softwareSystemBoundary == null)
                    {
                        softwareSystemBoundary = new SoftwareSystemBoundary(softwareSystem.Alias, softwareSystem.Label)
                        {
                            Structures = new List<Structure>()
                        };

                        structures.Add(softwareSystemBoundary);
                    }

                    if (!softwareSystemBoundary.Structures.Any(x => x.Alias == containerBoundary.Alias))
                    {
                        ((List<Structure>)softwareSystemBoundary.Structures).Add(containerBoundary);
                    }
                }
                else
                {
                    if (!structures.OfType<Component>().Any(x => x.Alias == interfaceOwner.Alias))
                    {
                        structures.Add(interfaceOwner);
                    }
                }
            }

            foreach (var usesInterface in @interface.Flow?.GetUsesInterfaces() ?? Enumerable.Empty<Interface>())
            {
                PopulateStructures(structures, usesInterface);
            }
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
                        foreach (var activity in Process.Activities)
                        {
                            foreach (var @interface in activity.Flow.GetUseFlows().Select(x => Utils.GetInstance<Interface>(x.Expression)).Where(x => x != null))
                            {
                                PopulateRelationships(_relationships, activity.GetActorInstance() ?? new SoftwareSystems.ExternalSystem.Interfaces.ExternalInterface().Instance, @interface);
                            }
                        }

                        _relationships = CleanUpRelationships(_relationships, MaxLineLabels, IsStatic).ToList();
                    }

                    return _relationships;
                }
            }
        }

        private void PopulateRelationships(IList<Relationship> relationships, Structure actor, Interface usesInterface)
        {
            if (actor is Interface interfaceActor)
            {
                actor = Utils.GetInstance<Structure>(interfaceActor.Owner);
            }

            var usesInterfaceOwner = Utils.GetInstance<Structure>(usesInterface.Owner);

            var input = Utils.GetInstance<Structure>(usesInterface.Input);
            var output = Utils.GetInstance<Structure>(usesInterface.Output);
            var outputTemplate = Utils.GetInstance<Structure>(usesInterface.OutputTemplate);

            var inputLabel = ShowInterfaceInputAndOutput ? (input != null ? $"{input.Label} -> " : string.Empty) : string.Empty;
            var outputLabel = ShowInterfaceInputAndOutput ? ($"{(outputTemplate != null || output != null ? " -> " : string.Empty)}{(outputTemplate != null ? $"{outputTemplate.Label}<" : string.Empty)}{(output != null ? output.Label : string.Empty)}{(outputTemplate != null ? ">" : string.Empty)}") : string.Empty;
            var label = $"{inputLabel}{usesInterface.Label}{outputLabel}";

            if (relationships.Where(x => x.From == (actor).Alias &&
                                        x.To == usesInterfaceOwner.Alias &&
                                        x.Label == label).FirstOrDefault() == null)
            {


                relationships.Add(((actor) > usesInterfaceOwner)[
                    label,
                    usesInterface.Protocol].AddTags(usesInterface.Tags?.ToArray()));
            }


            foreach (var usesAnotherInterface in usesInterface.Flow?.GetUsesInterfaces() ?? Enumerable.Empty<Interface>())
            {
                PopulateRelationships(relationships, usesInterface, usesAnotherInterface);
            }
        }
    }
}