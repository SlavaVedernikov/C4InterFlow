
using C4InterFlow.Structures.Interfaces;
using System.Text.RegularExpressions;

namespace C4InterFlow.Structures
{    
    public record Flow
    {
        private static readonly Regex ComponentsRegex = new Regex(@"\.Components\.[^.]*");
        private static readonly Regex ContainersRegex = new Regex(@"\.Containers\.[^.]*");

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
            Expression = @params;
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
        public string? Expression { get; set; }
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
            return GetUseFlows().Select(x => !string.IsNullOrEmpty(x.Expression) ? x.Expression : string.Empty).ToArray();
        }
        public Flow[] GetUseFlows()
        {
            var result = new List<Flow>();

            result.AddRange(GetFlowsByType(this, FlowType.Use));

            return result.ToArray();
        }

        internal Flow[] GetFlowsByType(Flow flow, FlowType type, bool isRecursive = true)
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

                    if(isRecursive)
                    {
                        foreach (var useSegment in segment.Flows)
                        {
                            result.AddRange(GetFlowsByType(useSegment, type));
                        }
                    }
                }
                else
                {
                    if (isRecursive)
                    {
                        result.AddRange(GetFlowsByType(segment, type));
                    }
                        
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
                if(x.Flows == null || x.Flows.Count == 0 || ComponentsRegex.IsMatch(x.Owner))
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
                ComponentsRegex.IsMatch(x.Owner) ||
                ContainersRegex.IsMatch(x.Owner))
                {
                    x.Type = FlowType.None;
                }
            });
        }
        private void InferContainerInterfaces(Flow flow, string currentScope)
        {
            if (flow.Type == FlowType.Use)
            {
                var inferredExpression = ComponentsRegex.Replace(flow.Expression, string.Empty);
                var inferredOwner = ComponentsRegex.Replace(flow.Owner, string.Empty);

                if (flow.Expression.Equals(inferredExpression) && flow.Owner.Equals(inferredOwner))
                {
                    if (flow.Flows != null)
                    {
                        foreach (var segment in flow.Flows)
                        {
                            InferContainerInterfaces(segment, currentScope);
                        }
                    }

                    return;
                }

                flow.Expression = inferredExpression;
                flow.SetOwner(inferredOwner, true);

                var newScope = Utils.GetContainerAlias(flow.Expression);

                if(string.IsNullOrEmpty(newScope))
                {
                    newScope = Utils.GetSoftwareSystemAlias(flow.Expression);
                }

                if (string.IsNullOrEmpty(newScope))
                {
                    newScope = flow.Expression;
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
                if(ComponentsRegex.IsMatch(flow.Owner))
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
                var inferredExpression = ComponentsRegex.Replace(flow.Expression, string.Empty);
                var inferredOwner = ComponentsRegex.Replace(flow.Owner, string.Empty);

                inferredExpression = ContainersRegex.Replace(inferredExpression, string.Empty);
                inferredOwner = ContainersRegex.Replace(inferredOwner, string.Empty);

                if (flow.Expression.Equals(inferredExpression) && flow.Owner.Equals(inferredOwner))
                {
                    if (flow.Flows != null)
                    {
                        foreach (var segment in flow.Flows)
                        {
                            InferSoftwareSystemInterfaces(segment, currentScope);
                        }
                    }

                    return;
                }
                    
                flow.Expression = inferredExpression;
                flow.SetOwner(inferredOwner, true);


                var newScope = Utils.GetSoftwareSystemAlias(flow.Expression);

                if (string.IsNullOrEmpty(newScope))
                {
                    newScope = flow.Expression;
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
                if (ComponentsRegex.IsMatch(flow.Owner) || ContainersRegex.IsMatch(flow.Owner))
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

        private bool IsAllowed(FlowType parentFlowType, FlowType childFlowType, out string message) {
            var result = false;
            message = string.Empty;
            FlowType[] allowedParents = new FlowType[] { };

            switch (childFlowType)
            {
                case FlowType.If:
                    allowedParents = new FlowType[] {
                        FlowType.None,
                        FlowType.If,
                        FlowType.ElseIf,
                        FlowType.Else,
                        FlowType.Loop,
                        FlowType.Group,
                        FlowType.Try,
                        FlowType.Catch,
                        FlowType.Finally};
                    break;
                case FlowType.ElseIf:
                    allowedParents = new FlowType[] {
                        FlowType.If};
                    break;
                case FlowType.Else:
                    allowedParents = new FlowType[] {
                        FlowType.If};
                    break;
                case FlowType.Loop:
                    allowedParents = new FlowType[] {
                        FlowType.None,
                        FlowType.If,
                        FlowType.ElseIf,
                        FlowType.Else,
                        FlowType.Loop,
                        FlowType.Group,
                        FlowType.Try,
                        FlowType.Catch,
                        FlowType.Finally};
                    break;
                case FlowType.Group:
                    allowedParents = new FlowType[] {
                        FlowType.None,
                        FlowType.If,
                        FlowType.ElseIf,
                        FlowType.Else,
                        FlowType.Loop,
                        FlowType.Group,
                        FlowType.Try,
                        FlowType.Catch,
                        FlowType.Finally};
                    break;
                case FlowType.Try:
                    allowedParents = new FlowType[] {
                        FlowType.None,
                        FlowType.If,
                        FlowType.ElseIf,
                        FlowType.Else,
                        FlowType.Loop,
                        FlowType.Group,
                        FlowType.Try,
                        FlowType.Catch,
                        FlowType.Finally};
                    break;
                case FlowType.Catch:
                    allowedParents = new FlowType[] {
                        FlowType.Try};
                    break;
                case FlowType.Finally:
                    allowedParents = new FlowType[] {
                        FlowType.Try};
                    break;
                case FlowType.ThrowException:
                    allowedParents = new FlowType[] {
                        FlowType.None,
                        FlowType.If,
                        FlowType.ElseIf,
                        FlowType.Else,
                        FlowType.Loop,
                        FlowType.Group,
                        FlowType.Try,
                        FlowType.Catch,
                        FlowType.Finally};
                    break;
                case FlowType.Return:
                    allowedParents = new FlowType[] {
                        FlowType.None,
                        FlowType.If,
                        FlowType.ElseIf,
                        FlowType.Else,
                        FlowType.Loop,
                        FlowType.Group,
                        FlowType.Try,
                        FlowType.Catch,
                        FlowType.Finally};
                    break;
                case FlowType.Use:
                    allowedParents = new FlowType[] {
                        FlowType.None,
                        FlowType.If,
                        FlowType.ElseIf,
                        FlowType.Else,
                        FlowType.Loop,
                        FlowType.Group,
                        FlowType.Try,
                        FlowType.Catch,
                        FlowType.Finally };
                    break;
            }

            result = allowedParents.Contains(parentFlowType);

            if(!result && allowedParents?.Count() > 0)
            {
                message = $"'{childFlowType}' is only allowed inside {string.Join(", ", allowedParents.Select(x => $"'{x}'"))}";
            }

            return result;
        }
        public Flow Return(string value)
        {
            var flowType = FlowType.Return;
            var parent = this;

            if(!IsAllowed(parent.Type, flowType, out var message))
            {
                throw new InvalidOperationException(message);
            }

            AddFlow(new Flow(flowType, parent, value));
            return this;
        }

        public Flow Use<T>() where T : IInterfaceInstance
        {
            return Use(Interface.GetAlias<T>());
        }
        public Flow Use(string interfaceAlias)
        {
            var flowType = FlowType.Use;
            var parent = this;

            if (!IsAllowed(parent.Type, flowType, out var message))
            {
                throw new InvalidOperationException(message);
            }

            AddFlow(new Flow(flowType, parent, interfaceAlias));
            return this;
        }

        public Flow ThrowException(string exception)
        {
            var flowType = FlowType.ThrowException;
            var parent = this;

            if (!IsAllowed(parent.Type, flowType, out var message))
            {
                throw new InvalidOperationException(message);
            }

            AddFlow(new Flow(flowType, parent, exception));
            return this;
        }

        public Flow If(string condition)
        {
            var flowType = FlowType.If;
            var parent = this;

            if (!IsAllowed(parent.Type, flowType, out var message))
            {
                throw new InvalidOperationException(message);
            }

            var flow = new Flow(flowType, this, condition);
            AddFlow(flow);
            return flow;
        }

        public Flow ElseIf(string condition)
        {
            var flowType = FlowType.ElseIf;
            var parent = Type == FlowType.ElseIf ? Parent : this;

            if (!IsAllowed(parent.Type, flowType, out var message))
            {
                throw new InvalidOperationException(message);
            }

            var flow = new Flow(flowType, parent, condition);
            parent.AddFlow(flow);
            return flow;
        }

        public Flow Else()
        {
            var flowType = FlowType.Else;
            var parent = Type == FlowType.ElseIf ? Parent : this;

            if (!IsAllowed(parent.Type, flowType, out var message))
            {
                throw new InvalidOperationException(message);
            }

            var flow = new Flow(flowType, parent);
            AddFlow(flow);
            return flow;
        }

        public Flow EndIf()
        {
            if (Type != FlowType.If && Type != FlowType.ElseIf && Type != FlowType.Else)
                throw new Exception("EndIf has to have the corresponding If");

            return Type == FlowType.If ? Parent : Parent.Parent;
        }

        public Flow Try()
        {
            var flowType = FlowType.Try;
            var parent = this;

            if (!IsAllowed(parent.Type, flowType, out var message))
            {
                throw new InvalidOperationException(message);
            }

            var flow = new Flow(flowType, parent);
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
            var flowType = FlowType.Catch;
            var parent = this;

            if (!IsAllowed(parent.Type, flowType, out var message))
            {
                throw new InvalidOperationException(message);
            }

            var flow = new Flow(flowType, parent, exception??string.Empty);
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
            var flowType = FlowType.Finally;
            var parent = this;

            if (!IsAllowed(parent.Type, flowType, out var message))
            {
                throw new InvalidOperationException(message);
            }

            var flow = new Flow(flowType, parent);
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
            var flowType = FlowType.Loop;
            var parent = this;

            if (!IsAllowed(parent.Type, flowType, out var message))
            {
                throw new InvalidOperationException(message);
            }

            var flow = new Flow(flowType, parent, condition);
            AddFlow(flow);
            return flow;
        }

        public Flow EndLoop()
        {
            if (Type != FlowType.Loop)
                throw new Exception("EndLoop has to have the corresponding Loop");

            return Parent;
        }

        public Flow Group(string label, string? ownerAlias = null)
        {
            var flowType = FlowType.Group;
            var parent = this;

            if (!IsAllowed(parent.Type, flowType, out var message))
            {
                throw new InvalidOperationException(message);
            }

            var flow = (ownerAlias == null ?
                new Flow(flowType, parent, label) :
                new Flow(flowType, parent, ownerAlias, label));
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
