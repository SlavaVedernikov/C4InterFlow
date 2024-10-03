using C4InterFlow.Visualisation.Plantuml.Style;
using C4InterFlow.Visualisation.Interfaces;
using C4InterFlow.Structures;
using C4InterFlow.Structures.Relationships;
using C4InterFlow.Structures.Boundaries;

namespace C4InterFlow.Visualisation
{

    public class EntityDiagram : DiagramBuildRunner
    {
        public EntityDiagram(string title, IEnumerable<Container> containers)
        {
            DiagramTitle = title;
            Containers = containers;
        }

        public EntityDiagram(string title, BusinessProcess process)
        {
            DiagramTitle = title;

            var interfaces = new List<Interface>();
            foreach (var activity in process.Activities)
            {
                interfaces.AddRange(activity.Flow?.GetUseFlows()?.Select(x => Utils.GetInstance<Interface>(x.Expression)) ?? Enumerable.Empty<Interface>());
            }

            Interfaces = interfaces;
        }
        private IEnumerable<Container> Containers { get; }

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
                        _structures = new List<Structure>();

                        if (Containers != null)
                        {
                            foreach (var container in Containers)
                            {
                                var entities = Utils.GetNestedInstances<Entity>($"{container.Alias}.Entities");

                                foreach (var item in entities)
                                {
                                    AddToStructures(item);
                                }
                            }
                        }

                        if (Interfaces != null)
                        {
                            foreach (var @interface in Interfaces)
                            {
                                AddToStructures(@interface);
                            }
                        }
                    }

                    return _structures;
                }
            }
        }

        private void AddToStructures(Interface @interface)
        {
            if (@interface == null) return;

            AddToStructures(Utils.GetInstance<Entity>(@interface.Input));

            AddToStructures(Utils.GetInstance<Entity>(@interface.Output));

            foreach (var usesInterface in @interface.Flow?.GetUsesInterfaces() ?? Enumerable.Empty<Interface>())
            {
                AddToStructures(usesInterface);
            }

        }

        private void AddToStructures(Entity entity)
        {
            if (entity == null) return;

            var container = Utils.GetInstance<Container>(entity.Container);

            if (container != null)
            {
                var packageBoundary = EnsurePackageBoundary(container);
                if (packageBoundary != null && packageBoundary.Entities.Where(x => x.Alias == entity.Alias).FirstOrDefault() == null)
                {
                    packageBoundary.Entities.Add(entity);
                }
            }

            if (entity.ComposedOfMany != null)
            {
                foreach (var composedOfManyItem in entity.ComposedOfMany)
                {
                    AddToStructures(Utils.GetInstance<Entity>(composedOfManyItem));
                }
            }

            if (entity.ComposedOfOne != null)
            {
                foreach (var composedOfOneItem in entity.ComposedOfOne)
                {
                    AddToStructures(Utils.GetInstance<Entity>(composedOfOneItem));
                }
            }

            AddToStructures(Utils.GetInstance<Entity>(entity.Extends));

        }
        private PackageBoundary EnsurePackageBoundary(Container container)
        {
            var result = _structures.Where(x => x.Alias == container.Alias).FirstOrDefault() as PackageBoundary;

            if (result == null)
            {
                result = new PackageBoundary(container.Alias, container.Label);
                _structures.Add(result);
            }

            return result;
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

                        foreach (var packageBoundary in Structures.Select(x => x as PackageBoundary))
                        {
                            if (packageBoundary != null)
                            {
                                foreach (var entity in packageBoundary.Entities.Select(x => x as Entity))
                                {
                                    if (entity != null)
                                    {
                                        if (entity.ComposedOfMany != null)
                                        {
                                            foreach (var composedOfManyItem in entity.ComposedOfMany)
                                            {
                                                var composedOfManyEntity = Utils.GetInstance<Entity>(composedOfManyItem);
                                                if (composedOfManyEntity != null)
                                                {
                                                    _relationships.Add((entity < composedOfManyEntity)[
                                                        $"composed of (0...*) >",
                                                        "composition"]);
                                                }
                                            }
                                        }

                                        if (entity.ComposedOfOne != null)
                                        {
                                            foreach (var composedOfOneItem in entity.ComposedOfOne)
                                            {
                                                var composedOfOneEntity = Utils.GetInstance<Entity>(composedOfOneItem);
                                                if (composedOfOneEntity != null)
                                                {
                                                    _relationships.Add((entity < composedOfOneEntity)[
                                                        $"composed of (0...1) >",
                                                        "composition"]);
                                                }
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(entity.Extends))
                                        {
                                            var extendsEntity = Utils.GetInstance<Entity>(entity.Extends);
                                            if (extendsEntity != null)
                                            {
                                                _relationships.Add((entity > extendsEntity)[
                                                    $"extends >",
                                                    "extension"]);
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }

                    return _relationships;
                }
            }
        }
    }
}