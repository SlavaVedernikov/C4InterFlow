
using System.Text.RegularExpressions;

namespace C4InterFlow.Elements
{    
    public record Flow
    {
        public enum FlowType
        {
            None,
            If,
            ElseIf,
            Else,
            Loop,
            Group,
            Try,
            Catch,
            Finally,
            ThrowException,
            Return,
            Use
        }

        public Flow() : this(string.Empty)
        {
        }
        public Flow(string owner)
        {
            Type = FlowType.None;
            Owner = owner;
        }

        private Flow(FlowType type, Flow parent, string? @params = null) : this(type, parent, parent.Owner, @params)
        {
            
        }

        private Flow(FlowType type, Flow parent, string owner, string? @params = null)
        {
            Owner = owner;
            Type = type;
            Parent = parent;
            Params = @params;
        }

        public IList<Flow> Flows { get; set; } = new List<Flow>();
        public FlowType Type { get; set; }
        private Flow? Parent { get; set; }

        private string _owner = string.Empty;
        public string Owner { 
            get 
            {
                return _owner;
            }
            set
            {
                _owner = value;

                if(Flows != null)
                {
                    foreach (var flow in Flows)
                    {
                        flow.Owner = value;
                    }
                }
            } 
        }

        private void SetOwner(string value, bool selfOnly = false)
        {
            if (selfOnly)
            {
                _owner = value;
            }
            else
            {
                Owner = value;
            }
        }
        public string? Params { get; set; }
        public Interface[] GetUsesInterfaces()
        {
            var result = new List<Interface>();
            var interfaceAliases = GetUsesInterfaceAliases();

            foreach (var interfaceAlias in interfaceAliases)
            {
                var interfaceInstance = Utils.GetInstance<Interface>(interfaceAlias);
                if (interfaceInstance != null)
                {
                    result.Add(interfaceInstance);
                }
            }

            return result.ToArray();

        }

        private string[] GetUsesInterfaceAliases()
        {
            return GetUseFlows().Select(x => !string.IsNullOrEmpty(x.Params) ? x.Params : string.Empty).ToArray();
        }
        public Flow[] GetUseFlows()
        {
            var result = new List<Flow>();

            result.AddRange(GetFlowsByType(this, FlowType.Use));

            return result.ToArray();
        }

        internal Flow[] GetFlowsByType(Flow flow, FlowType type)
        {
            var result = new List<Flow>();

            if (flow?.Flows == null)
            {
                return result.ToArray();
            }

            foreach (var segment in flow.Flows)
            {
                if (segment.Type == type)
                {
                    result.Add(segment);
                    
                    if (segment.Flows == null) continue;

                    foreach (var useSegment in segment.Flows)
                    {
                        result.AddRange(GetFlowsByType(useSegment, type));
                    }
                }
                else
                {
                    result.AddRange(GetFlowsByType(segment, type));
                }
            }

            return result.ToArray();
        }
        public void AddFlow(Flow flow)
        {
            Flows.Add(flow);
        }

        public void AddFlowsRange(IEnumerable<Flow> flows)
        {
            if (flows == null) return;

            foreach (var segment in flows)
            {
                Flows.Add(segment);
            }
        }

        public void InferContainerInterfaces()
        {
            var currentScope = Utils.GetContainerAlias(Owner);

            if (string.IsNullOrEmpty(currentScope))
            {
                currentScope = Utils.GetSoftwareSystemAlias(Owner);
            }

            if (string.IsNullOrEmpty(currentScope))
            {
                currentScope = Owner;
            }

            InferContainerInterfaces(this, currentScope);
            CleanUpInferredContainerFlows();

        }

        private void CleanUpInferredContainerFlows()
        {
            var flows = new List<Flow>();

            var json = System.Text.Json.JsonSerializer.Serialize(this);

            flows.AddRange(GetFlowsByType(this, FlowType.If));
            flows.AddRange(GetFlowsByType(this, FlowType.ElseIf));
            flows.AddRange(GetFlowsByType(this, FlowType.Else));
            flows.AddRange(GetFlowsByType(this, FlowType.Loop));
            flows.AddRange(GetFlowsByType(this, FlowType.Group));
            flows.AddRange(GetFlowsByType(this, FlowType.Try));
            flows.AddRange(GetFlowsByType(this, FlowType.Catch));
            flows.AddRange(GetFlowsByType(this, FlowType.Finally));

            flows.ForEach(x =>
            {
                if(x.Flows == null || x.Flows.Count == 0 || x.Owner.Contains(".Components."))
                {
                    x.Type = FlowType.None;
                }
            });
        }

        public void InferSoftwareSystemInterfaces()
        {
            var currentScope = Utils.GetSoftwareSystemAlias(Owner);

            if (string.IsNullOrEmpty(currentScope))
            {
                currentScope = Owner;
            }

            InferSoftwareSystemInterfaces(this, currentScope);
            CleanUpInferredSoftwareSystemFlows();

        }

        private void CleanUpInferredSoftwareSystemFlows()
        {
            var flows = new List<Flow>();

            var json = System.Text.Json.JsonSerializer.Serialize(this);

            flows.AddRange(GetFlowsByType(this, FlowType.If));
            flows.AddRange(GetFlowsByType(this, FlowType.ElseIf));
            flows.AddRange(GetFlowsByType(this, FlowType.Else));
            flows.AddRange(GetFlowsByType(this, FlowType.Loop));
            flows.AddRange(GetFlowsByType(this, FlowType.Group));
            flows.AddRange(GetFlowsByType(this, FlowType.Try));
            flows.AddRange(GetFlowsByType(this, FlowType.Catch));
            flows.AddRange(GetFlowsByType(this, FlowType.Finally));

            flows.ForEach(x =>
            {
                if (x.Flows == null || 
                x.Flows.Count == 0 || 
                x.Owner.Contains(".Components.") || 
                x.Owner.Contains(".Containers."))
                {
                    x.Type = FlowType.None;
                }
            });
        }
        private void InferContainerInterfaces(Flow flow, string currentScope)
        {
            if(flow.Type == FlowType.Use)
            {
                flow.Params = new Regex(@"\.Components\.[^.]*").Replace(flow.Params, string.Empty);
                flow.SetOwner(new Regex(@"\.Components\.[^.]*").Replace(flow.Owner, string.Empty), true);

                var newScope = Utils.GetContainerAlias(flow.Params);

                if(string.IsNullOrEmpty(newScope))
                {
                    newScope = Utils.GetSoftwareSystemAlias(flow.Params);
                }

                if (string.IsNullOrEmpty(newScope))
                {
                    newScope = flow.Params;
                }

                if(currentScope.Equals(newScope))
                {
                    flow.Type = FlowType.None;
                }

                if (flow.Flows != null)
                {
                    foreach (var segment in flow.Flows)
                    {
                        InferContainerInterfaces(segment, newScope);
                    }
                }
            }
            else if (flow.Type == FlowType.Return || flow.Type == FlowType.ThrowException) 
            {
                if(flow.Owner.Contains(".Components."))
                {
                    flow.Type = FlowType.None;
                }
            }
            else
            {
                if (flow.Flows != null)
                {
                    foreach (var segment in flow.Flows)
                    {
                        InferContainerInterfaces(segment, currentScope);
                    }
                }
            }
        }

        private void InferSoftwareSystemInterfaces(Flow flow, string currentScope)
        {
            if (flow.Type == FlowType.Use)
            {
                flow.Params = new Regex(@"\.Components\.[^.]*").Replace(flow.Params, string.Empty);
                flow.SetOwner(new Regex(@"\.Components\.[^.]*").Replace(flow.Owner, string.Empty), true);

                flow.Params = new Regex(@"\.Containers\.[^.]*").Replace(flow.Params, string.Empty);
                flow.SetOwner(new Regex(@"\.Containers\.[^.]*").Replace(flow.Owner, string.Empty), true);

                var newScope = Utils.GetSoftwareSystemAlias(flow.Params);

                if (string.IsNullOrEmpty(newScope))
                {
                    newScope = flow.Params;
                }

                if (currentScope.Equals(newScope))
                {
                    flow.Type = FlowType.None;
                }

                if (flow.Flows != null)
                {
                    foreach (var segment in flow.Flows)
                    {
                        InferSoftwareSystemInterfaces(segment, newScope);
                    }
                }
            }
            else if (flow.Type == FlowType.Return || flow.Type == FlowType.ThrowException)
            {
                if (flow.Owner.Contains(".Components.") || flow.Owner.Contains(".Containers."))
                {
                    flow.Type = FlowType.None;
                }
            }
            else
            {
                if (flow.Flows != null)
                {
                    foreach (var segment in flow.Flows)
                    {
                        InferSoftwareSystemInterfaces(segment, currentScope);
                    }
                }
            }
        }
        /*
        public void InferContainerInterfaces()
        {
            var useFlows = GetUseFlows();

            useFlows.Where(x => x.Params.Contains(".Components.") || x.Owner.Contains(".Components."))
                .ToList()
                .ForEach(x =>
                {
                    x.Params = new Regex(@"\.Components\.[^.]*").Replace(x.Params, string.Empty);
                    x.SetOwner(new Regex(@"\.Components\.[^.]*").Replace(x.Owner, string.Empty), true);
                });

            var otherFlows = new List<Flow>();

            otherFlows.AddRange(GetFlowsByType(this, FlowType.Return));
            otherFlows.AddRange(GetFlowsByType(this, FlowType.ThrowException));

            otherFlows.Where(x => x.Owner.Contains(".Components."))
                .ToList()
                .ForEach(x =>
                {
                    x.SetOwner(new Regex(@"\.Components\.[^.]*").Replace(x.Owner, string.Empty), true);
                });
        }
        */

        /*
        public void InferSoftwareSystemInterfaces()
        {

            var useFlows = GetUseFlows();

            useFlows.Where(x => x.Params.Contains(".Components.") || x.Owner.Contains(".Components."))
                .ToList()
                .ForEach(x =>
                {
                    x.Params = new Regex(@"\.Components\.[^.]*").Replace(x.Params, string.Empty);
                    x.SetOwner(new Regex(@"\.Components\.[^.]*").Replace(x.Owner, string.Empty), true);
                });

            useFlows.Where(x => x.Params.Contains(".Containers.") || x.Owner.Contains(".Containers."))
                .ToList()
                .ForEach(x =>
                {
                    x.Params = new Regex(@"\.Containers\.[^.]*").Replace(x.Params, string.Empty);
                    x.SetOwner(new Regex(@"\.Containers\.[^.]*").Replace(x.Owner, string.Empty), true);
                });

            var otherFlows = new List<Flow>();

            otherFlows.AddRange(GetFlowsByType(this, FlowType.Return));
            otherFlows.AddRange(GetFlowsByType(this, FlowType.ThrowException));

            otherFlows.Where(x => x.Owner.Contains(".Components."))
                .ToList()
                .ForEach(x =>
                {
                    x.SetOwner(new Regex(@"\.Components\.[^.]*").Replace(x.Owner, string.Empty), true);
                });

            otherFlows.Where(x => x.Owner.Contains(".Containers."))
                .ToList()
                .ForEach(x =>
                {
                    x.SetOwner(new Regex(@"\.Containers\.[^.]*").Replace(x.Owner, string.Empty), true);
                });
        
        */

        public Flow Return(string value)
        {
            var flow = new Flow(FlowType.Return, this, value);
            AddFlow(flow);
            return this;
        }

        public Flow Use(string interfaceAlias)
        {
            var flow = new Flow(FlowType.Use, this, interfaceAlias);
            AddFlow(flow);
            return this;
        }

        public Flow ThrowException(string exception)
        {
            var flow = new Flow(FlowType.ThrowException, this, exception);
            AddFlow(flow);
            return this;
        }

        public Flow If(string condition)
        {
            var flow = new Flow(FlowType.If, this, condition);
            AddFlow(flow);
            return flow;
        }

        public Flow ElseIf(string condition)
        {
            if (Type != FlowType.If && Type != FlowType.ElseIf)
                throw new Exception("ElseIf has to have corresponding If or ElseIf");

            var flow = new Flow(FlowType.ElseIf, Parent, condition);
            AddFlow(flow);
            return flow;
        }

        public Flow Else()
        {
            if (Type != FlowType.If && Type != FlowType.ElseIf)
                throw new Exception("Else has to have corresponding If or ElseIf");

            var flow = new Flow(FlowType.Else, Parent);
            AddFlow(flow);
            return flow;
        }

        public Flow EndIf()
        {
            if (Type != FlowType.If && Type != FlowType.ElseIf && Type != FlowType.Else)
                throw new Exception("EndIf has to have the corresponding If");

            return Parent;
        }

        public Flow Try()
        {
            var flow = new Flow(FlowType.Try, this);
            AddFlow(flow);
            return flow;
        }

        public Flow EndTry()
        {
            if (Type != FlowType.Try)
                throw new Exception("EndTry has to have the corresponding Try");

            return Parent;
        }

        public Flow Catch(string? exception = null)
        {
            if (Type != FlowType.Try)
                throw new Exception("Catch has to have the corresponding Try");

            var flow = new Flow(FlowType.Catch, this, exception??string.Empty);
            AddFlow(flow);
            return flow;
        }

        public Flow EndCatch()
        {
            if (Type != FlowType.Catch)
                throw new Exception("EndCatch has to have the corresponding Catch");

            return Parent;
        }

        public Flow Finally()
        {
            if (Type != FlowType.Try)
                throw new Exception("Finally has to have the corresponding Try");

            var flow = new Flow(FlowType.Finally, this);
            AddFlow(flow);
            return flow;
        }

        public Flow EndFinally()
        {
            if (Type != FlowType.Finally)
                throw new Exception("EndFinally has to have the corresponding Finally");

            return Parent;
        }

        public Flow Loop(string condition)
        {
            var flow = new Flow(FlowType.Loop, this, condition);
            AddFlow(flow);
            return flow;
        }

        public Flow EndLoop()
        {
            if (Type != FlowType.Loop)
                throw new Exception("EndLoop has to have the corresponding Loop");

            return Parent;
        }

        public Flow Group(string condition, string? ownerAlias = null)
        {
            var flow = (ownerAlias == null ?
                new Flow(FlowType.Group, this, condition) :
                new Flow(FlowType.Group, this, ownerAlias, condition));
            AddFlow(flow);
            return flow;
        }

        public Flow EndGroup()
        {
            if (Type != FlowType.Group)
                throw new Exception("EndGroup has to have the corresponding Group");

            return Parent;
        }
    }
}
