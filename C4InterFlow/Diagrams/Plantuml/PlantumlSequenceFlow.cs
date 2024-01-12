using C4InterFlow.Elements;
using C4InterFlow.Elements.Relationships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow.Diagrams.Plantuml
{
    public static class PlantumlSequenceFlow
    {
        public static string ToPumlSequenceString(this Flow flow)
        {
            var sb = new StringBuilder();
            var pumlKeyword = string.Empty;
            var flowOwner = default(Structure);
            var actor = default(Structure);

            if(!string.IsNullOrEmpty(flow.OwnerAlias))
            {
                flowOwner = Utils.GetInstance<Structure>(flow.OwnerAlias);
                if (flowOwner != null)
                {
                    if(flowOwner is Interface @interface)
                    {
                        actor = Utils.GetInstance<Structure>(@interface.Owner);
                    }
                    else
                    {
                        actor = flowOwner;
                    }
                    
                }
            }
            

            switch (flow.Type)
            {
                case Flow.FlowType.If:
                    pumlKeyword = "alt";
                    break;
                case Flow.FlowType.ElseIf:
                case Flow.FlowType.Else:
                    pumlKeyword = "else";
                    break;
                case Flow.FlowType.Loop:
                    pumlKeyword = "loop";
                    break;
                case Flow.FlowType.Group:
                    pumlKeyword = "group";
                    break;
                case Flow.FlowType.None:
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(pumlKeyword))
            {
                sb.AppendLine($"{pumlKeyword} {(!string.IsNullOrEmpty(flow.Params) ? flow.Params.Replace("\n", @"\n") : string.Empty)}");
            }

            if (flow.Type == Flow.FlowType.ThrowException)
            {
                var flowRelationship = new Relationship(actor.Alias, actor.Alias, $"Exception{(!string.IsNullOrEmpty(flow.Params) ? $" ({flow.Params.Replace("\n", @"\n")})" : string.Empty)}");
                sb.AppendLine(flowRelationship.ToPumlSequenceString());
            }
            else if (flow.Type == Flow.FlowType.Return)
            {
                var flowRelationship = new Relationship(actor.Alias, actor.Alias, $"Retun{(!string.IsNullOrEmpty(flow.Params) ? $" ({flow.Params.Replace("\n", @"\n")})" : string.Empty)}");
                sb.AppendLine(flowRelationship.ToPumlSequenceString());
            }
            else if(flow.Type == Flow.FlowType.Use)
            {
                var usesInterface = Utils.GetInstance<Interface>(flow.Params);
                var usesInterfaceOwner = Utils.GetInstance<Structure>(usesInterface.Owner);
                var flowRelationship = new Relationship(actor.Alias, usesInterfaceOwner.Alias, usesInterface.Label);
                sb.AppendLine(flowRelationship.ToPumlSequenceString());
                
                if(flow.Flows.Any())
                {
                    sb.AppendLine($"group {usesInterface.Label}");
                }
                
            }

            foreach (var segment in flow.Flows)
            {
                sb.Append(segment.ToPumlSequenceString());
            }

            if (flow.Type == Flow.FlowType.If || 
                flow.Type == Flow.FlowType.Loop || 
                flow.Type == Flow.FlowType.Group ||
                (flow.Type == Flow.FlowType.Use && flow.Flows.Any()))
            {
                sb.AppendLine("end");
            }

            return sb.ToString();
        }
    }
}
