using C4InterFlow.Cli.Commands.Options;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Relationships;
using C4InterFlow.Visualisation.Interfaces;
using C4InterFlow.Visualisation.Plantuml.Style;
using static C4InterFlow.SoftwareSystems.ExternalSystem;

namespace C4InterFlow.Visualisation
{

    public class ContextDiagram : DiagramBuildRunner
    {
        public ContextDiagram(
            string title, 
            BusinessProcess process,
            int maxLineLabels = DiagramMaxLineLabelsOption.DefaultValue, 
            bool isStatic = false)
        {
            DiagramTitle = title;
            Process = process;
            MaxLineLabels = maxLineLabels;
            IsStatic = isStatic;
        }

        private int MaxLineLabels { get; init; }
        private bool IsStatic { get; init; }
        private BusinessProcess Process { get; init; }

        private string DiagramTitle { get; }
        protected override string Title => DiagramTitle;
        protected override DiagramType DiagramType => IsStatic ? DiagramType.ContextStatic : DiagramType.Context;
        protected override bool ShowLegend => true;

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

                _flow.InferSoftwareSystemInterfaces();

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
        }

        private IList<Structure> PopulateStructures(IList<Structure> structures, Interface @interface, string? currentScope = null)
        {
            var interfaceOwner = Utils.GetInstance<Structure>(@interface.Owner);

            if (interfaceOwner is SoftwareSystem &&
                !structures.OfType<SoftwareSystem>().Any(x => x?.Alias == interfaceOwner.Alias))
            {
                structures.Add(interfaceOwner);
                currentScope = interfaceOwner.Alias;
            }
            else if (interfaceOwner is Container)
            {
                var softwareSystem = Utils.GetInstance<Structure>(((Container)interfaceOwner).SoftwareSystem);

                if (currentScope != softwareSystem.Alias &&
                    !structures.OfType<SoftwareSystem>().Any(x => x?.Alias == softwareSystem.Alias))
                {
                    structures.Add(softwareSystem);
                    currentScope = softwareSystem.Alias;
                }
            }
            else if (interfaceOwner is Component)
            {
                var container = Utils.GetInstance<Structure>(((Component)interfaceOwner).Container);
                var softwareSystem = Utils.GetInstance<Structure>(((Container)container).SoftwareSystem);

                if (currentScope != softwareSystem.Alias &&
                    !structures.OfType<SoftwareSystem>().Any(x => x?.Alias == softwareSystem.Alias))
                {
                    structures.Add(softwareSystem);
                    currentScope = softwareSystem.Alias;
                }
            }

            foreach (var usesInterface in @interface.Flow?.GetUsesInterfaces() ?? Enumerable.Empty<Interface>())
            {
                PopulateStructures(structures, usesInterface, currentScope);
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

                        foreach (var activity in Process.Activities)
                        {
                            foreach (var flow in activity.Flow?.GetUseFlows())
                            {
                                var @interface = Utils.GetInstance<Interface>(flow.Expression);
                                if (@interface != null)
                                {
                                    PopulateRelationships(_relationships, activity.GetActorInstance() ?? new SoftwareSystems.ExternalSystem.Interfaces.ExternalInterface().Instance, @interface, isConditional: flow.IsConditional);
                                }
                            }
                        }

                        _relationships = CleanUpRelationships(_relationships, MaxLineLabels, IsStatic).ToList();
                    }

                    return _relationships;
                }
            }
        }

        private void PopulateRelationships(IList<Relationship> relationships, Structure actor, Interface usesInterface, string? fromScope = null, string? toScope = null, bool isConditional = false)
        {
            if (actor is Interface i)
            {
                actor = Utils.GetInstance<Structure>(i.Owner);
            }

            var usesInterfaceOwner = Utils.GetInstance<Structure>(usesInterface.Owner);
            var newFromScope = toScope;
            var newToScope = default(string?);

            if (actor is Container)
            {
                var softwareSystem = Utils.GetInstance<Structure>(((Container)actor).SoftwareSystem);
                if (softwareSystem != null)
                {
                    actor = softwareSystem;
                }
            }
            else if (actor is Component)
            {
                var container = Utils.GetInstance<Structure>(((Component)actor).Container);
                if (container != null)
                {
                    var softwareSystem = Utils.GetInstance<Structure>(((Container)container).SoftwareSystem);
                    if (softwareSystem != null)
                    {
                        actor = softwareSystem;
                    }
                }
            }

            if (usesInterfaceOwner is SoftwareSystem)
            {
                newToScope = usesInterfaceOwner.Alias;
            }
            else if (usesInterfaceOwner is Container)
            {
                var usesSoftwareSystem = Utils.GetInstance<Structure>(((Container)usesInterfaceOwner).SoftwareSystem);
                if (usesSoftwareSystem != null)
                {
                    newToScope = usesSoftwareSystem.Alias;
                    usesInterfaceOwner = usesSoftwareSystem;
                }
            }
            else if (usesInterfaceOwner is Component)
            {
                var usesContainer = Utils.GetInstance<Structure>(((Component)usesInterfaceOwner).Container);
                if (usesContainer != null)
                {
                    var usesSoftwareSystem = Utils.GetInstance<Structure>(((Container)usesContainer).SoftwareSystem);
                    if (usesSoftwareSystem != null)
                    {
                        newToScope = usesSoftwareSystem.Alias;
                        usesInterfaceOwner = usesSoftwareSystem;
                    }
                }
            }

            var label = $"{usesInterface.Label}{(isConditional ? " (Conditional)" : string.Empty)}";
            var protocol = $"{usesInterface.Protocol}";
            if (relationships.Where(x => x.From == (actor).Alias &&
                                        x.To == usesInterfaceOwner.Alias &&
                                        x.Label == label &&
                                        x.Protocol == protocol).FirstOrDefault() == null &&
                (!(newFromScope ?? string.Empty).Equals(newToScope)))
            {
                relationships.Add(((actor) > usesInterfaceOwner)[
                    label,
                    protocol].AddTags(usesInterface.Tags?.ToArray()));
            }

            foreach ( var flow in usesInterface.Flow?.GetUseFlows())
            {
                var usesAnotherInterface = Utils.GetInstance<Interface>(flow.Expression);
                if (usesAnotherInterface != null)
                {
                    PopulateRelationships(relationships, usesInterface, usesAnotherInterface, newFromScope, newToScope, flow.IsConditional);
                } 
            }
        }
    }
}