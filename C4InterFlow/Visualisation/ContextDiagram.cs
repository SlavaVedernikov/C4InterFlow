using C4InterFlow.Visualisation.Plantuml.Style;
using C4InterFlow.Visualisation.Interfaces;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Relationships;

namespace C4InterFlow.Visualisation
{

    public class ContextDiagram : DiagramBuildRunner
    {
        public ContextDiagram(string title, BusinessProcess process, bool isStatic = false)
        {
            DiagramTitle = title;
            Process = process;
            IsStatic = isStatic;
        }
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
                            var actor = activity.GetActorInstance() ?? SoftwareSystems.ExternalSystem.Interfaces.ExternalInterface.Instance;
                            //TODO: Consider refactoring this so that it is treated as a divider/separator e.g. "== {actor.Label} =="
                            parentFlow = _flow.Group(
                                $"{actor.Label}{(!string.IsNullOrEmpty(activity.Label) ? $" - {activity.Label}" : string.Empty)}",
                                actor.Alias);
                        }


                        var activityFlow = Utils.Clone(activity.Flow);
                        foreach (var useFlow in activityFlow.GetUseFlows())
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
            foreach (var useFlow in currentFlow.GetUseFlows())
            {
                PopulateFlow(useFlow);
            }

            flow.AddFlowsRange(currentFlow.Flows);
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
                            if (actor != null && !_structures.Any(i => i.Alias != actor.Alias))
                            {
                                _structures.Add(actor);
                            }

                            foreach (var @interface in activity.Flow.GetUsesInterfaces())
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

            foreach (var usesInterface in @interface.Flow.GetUsesInterfaces())
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
                            foreach (var @interface in activity.Flow.GetUseFlows().Select(x => Utils.GetInstance<Interface>(x.Expression)))
                            {
                                PopulateRelationships(_relationships, activity.GetActorInstance() ?? SoftwareSystems.ExternalSystem.Interfaces.ExternalInterface.Instance, @interface);
                            }
                        }

                        _relationships = CleanUpRelationships(_relationships, IsStatic).ToList();
                    }

                    return _relationships;
                }
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

            var label = $"{usesInterface.Label}";
            var protocol = $"{usesInterface.Protocol}";
            if (relationships.Where(x => x.From == (actor).Alias &&
                                        x.To == usesInterfaceOwner.Alias &&
                                        x.Label == label &&
                                        x.Protocol == protocol).FirstOrDefault() == null &&
                (!(fromScope ?? string.Empty).Equals(newToScope)))
            {
                relationships.Add(((actor) > usesInterfaceOwner)[
                    label,
                    protocol].AddTags(usesInterface.Tags?.ToArray()));
            }

            foreach (var usesAnotherInterface in usesInterface.Flow.GetUsesInterfaces())
            {
                PopulateRelationships(relationships, usesInterface, usesAnotherInterface, newFromScope, newToScope);
            }
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