using C4InterFlow.Visualisation.Plantuml.Style;
using C4InterFlow.Visualisation.Interfaces;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Relationships;
using C4InterFlow.Structures.Boundaries;
using C4InterFlow.Cli.Commands.Options;

namespace C4InterFlow.Visualisation
{
    public class ContainerDiagram : DiagramBuildRunner
    {
        public ContainerDiagram(
            string title, 
            BusinessProcess process, 
            bool showBoundaries = false,
            int maxLineLabels = DiagramMaxLineLabelsOption.DefaultValue,
            bool isStatic = false)
        {
            DiagramTitle = title;
            Process = process;
            ShowBoundaries = showBoundaries;
            MaxLineLabels = maxLineLabels;
            IsStatic = isStatic;
        }

        private int MaxLineLabels { get; init; }
        private bool IsStatic { get; init; }
        private BusinessProcess Process { get; init; }

        private string DiagramTitle { get; }
        protected override string Title => DiagramTitle;
        protected override DiagramType DiagramType => IsStatic ? DiagramType.ContainerStatic : DiagramType.Container;
        protected override bool ShowLegend => true;
        private bool ShowBoundaries { get; init; }

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

                        if (Process.Activities.Count() > 1)
                        {
                            var actor = activity.GetActorInstance() ?? new SoftwareSystems.ExternalSystem.Interfaces.ExternalInterface().Instance;
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

                        if (parentFlow.Type == Flow.FlowType.Group)
                        {
                            parentFlow.EndGroup();
                        }
                    }
                }

                _flow.InferContainerInterfaces();

                return _flow;
            }
        }

        private void PopulateFlow(Flow flow)
        {
            var usesInterface = Utils.GetInstance<Interface>(flow.Expression);

            if (usesInterface == null) return;
             
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
                {
                    if (_structures == null)
                    {
                        _structures = new List<Structure>();

                        foreach (var activity in Process.Activities)
                        {
                            var actor = activity.GetActorInstance();
                            if (actor != null && !_structures.Any(i => i.Alias == actor.Alias))
                            {
                                if(actor is Interface @interface)
                                {
                                    var interfaceOwner = Utils.GetInstance<Structure>(@interface.Owner);
                                    if(interfaceOwner != null)
                                    {
                                        _structures.Add(interfaceOwner);
                                    }
                                    else
                                    {
                                        _structures.Add(actor);
                                    }
                                }
                                else
                                {
                                    _structures.Add(actor);
                                }
                                
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
        }

        private void PopulateStructures(IList<Structure> structures, Interface @interface, string? currentScope = null)
        {
            var interfaceOwner = Utils.GetInstance<Structure>(@interface.Owner);

            if (interfaceOwner is SoftwareSystem &&
                !structures.OfType<SoftwareSystem>().Any(x => x.Alias == interfaceOwner.Alias))
            {
                structures.Add(interfaceOwner);
                currentScope = interfaceOwner.Alias;
            }
            else if (interfaceOwner is Container)
            {
                var container = interfaceOwner as Container;

                if (container != null)
                {
                    currentScope = AddContainer(structures, container, currentScope);
                }
            }
            else if (interfaceOwner is Component)
            {
                var container = Utils.GetInstance<Container>(((Component)interfaceOwner).Container);

                if (container != null && currentScope != container.Alias)
                {
                    currentScope = AddContainer(structures, container, currentScope);
                }
            }

            foreach (var usesInterface in @interface.Flow?.GetUsesInterfaces() ?? Enumerable.Empty<Interface>())
            {
                PopulateStructures(structures, usesInterface, currentScope);
            }
        }

        private string? AddContainer(IList<Structure> structures, Container container, string? currentScope)
        {
            var result = currentScope;

            if (ShowBoundaries)
            {
                var softwareSystem = Utils.GetInstance<SoftwareSystem>(container.SoftwareSystem);
                var softwareSystemBoundary = structures.OfType<SoftwareSystemBoundary>().FirstOrDefault(x => x.Alias == softwareSystem?.Alias);

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
                    result = container.Alias;
                }
            }
            else
            {
                if (!structures.OfType<Container>().Any(x => x.Alias == container.Alias))
                {
                    structures.Add(container);
                    result = container.Alias;
                }
            }

            return result;
        }

        private List<Relationship> _relationships;
        protected override IEnumerable<Relationship> Relationships
        {
            get
            {
                if (_relationships == null)
                {
                    _relationships = new List<Relationship>();

                    foreach (var activity in Process.Activities)
                    {
                        foreach (var @interface in activity.Flow?.GetUsesInterfaces() ?? Enumerable.Empty<Interface>())
                        {
                            PopulateRelationships(_relationships, activity.GetActorInstance() ?? new SoftwareSystems.ExternalSystem.Interfaces.ExternalInterface().Instance, @interface);
                        }
                    }

                    _relationships = CleanUpRelationships(_relationships, MaxLineLabels, IsStatic).ToList();
                }

                return _relationships;
            }
        }

        private void PopulateRelationships(IList<Relationship> relationships, Structure actor, Interface usesInterface, string? fromScope = null, string? toScope = null)
        {
            if (actor is Interface i)
            {
                actor = Utils.GetInstance<Structure>(i.Owner);
            }
            var usesInterfaceOwner = Utils.GetInstance<Structure>(usesInterface.Owner);

            var newFromScope = toScope;
            var newToScope = default(string?);

            if (actor is Component)
            {
                var container = Utils.GetInstance<Structure>(((Component)actor).Container);
                if (container != null)
                {
                    actor = container;
                }
            }

            
            if (usesInterfaceOwner is Component)
            {
                var usesContainer = Utils.GetInstance<Structure>(((Component)usesInterfaceOwner).Container);
                if (usesContainer != null)
                {
                    newToScope = usesContainer.Alias;
                    usesInterfaceOwner = usesContainer;
                }
            }
            else if (usesInterfaceOwner is Container || usesInterfaceOwner is SoftwareSystem)
            {
                newToScope = usesInterfaceOwner.Alias;
            }

            var label = $"{usesInterface.Label}";

            if (relationships.Where(x => x.From == (actor).Alias &&
                                        x.To == usesInterfaceOwner.Alias &&
                                        x.Label == label).FirstOrDefault() == null &&
                (!(newFromScope ?? string.Empty).Equals(newToScope)))
            {


                relationships.Add(((actor) > usesInterfaceOwner)[
                    label,
                    usesInterface.Protocol].AddTags(usesInterface.Tags?.ToArray()));
            }

            foreach (var usesAnotherInterface in usesInterface.Flow?.GetUsesInterfaces() ?? Enumerable.Empty<Interface>())
            {
                PopulateRelationships(relationships, usesInterface, usesAnotherInterface, newFromScope, newToScope);
            }
        }
    }
}