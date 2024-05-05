using C4InterFlow.Structures;
using C4InterFlow.Structures.Relationships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using C4InterFlow.Visualisation.Plantuml.Enums;

namespace C4InterFlow.Visualisation.Plantuml
{
    public static class PlantumlSequenceFlow
    {
        public static string ToPumlSequenceString(this Flow flow, SequenceDiagramStyle style)
        {
            var sb = new StringBuilder();
            var pumlKeyword = string.Empty;
            var flowOwner = default(Structure);
            var actor = default(Structure);
            var innerFlows = new List<Flow>();

            if (!string.IsNullOrEmpty(flow.Owner))
            {
                flowOwner = Utils.GetInstance<Structure>(flow.Owner);
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
                else
                {
                    // Flow Owner may be an inferred Interface, so get the corresponding structure from its Alias
                    actor = Utils.GetInstance<Structure>(new Regex(@"\.Interfaces\.[^.]*").Replace(flow.Owner, string.Empty));
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
                sb.AppendLine($"{pumlKeyword} {(!string.IsNullOrEmpty(flow.Expression) ? flow.Expression.Replace("\n", @"\n") : string.Empty)}");
            }

            if (flow.Type == Flow.FlowType.ThrowException)
            {
                var flowRelationship = new Relationship(actor.Alias, actor.Alias, $"Exception{(!string.IsNullOrEmpty(flow.Expression) ? $" ({flow.Expression.Replace("\n", @"\n")})" : string.Empty)}");
                sb.AppendLine(flowRelationship.ToPumlSequenceString(style));
            }
            else if (flow.Type == Flow.FlowType.Return)
            {
                var flowRelationship = new Relationship(actor.Alias, actor.Alias, $"Retun{(!string.IsNullOrEmpty(flow.Expression) ? $" ({flow.Expression.Replace("\n", @"\n")})" : string.Empty)}");
                sb.AppendLine(flowRelationship.ToPumlSequenceString(style));
            }
            else if(flow.Type == Flow.FlowType.Use)
            {
                var usesInterfaceOwner = default(Structure);
                var label = string.Empty;

                var usesInterface = Utils.GetInstance<Interface>(flow.Expression);
                if(usesInterface != null)
                {
                    usesInterfaceOwner = Utils.GetInstance<Structure>(usesInterface.Owner);
                    label = usesInterface.Label;
                }
                else
                {
                    // Flow may be using an inferred Interface, so get the corresponding structure from its Alias
                    usesInterfaceOwner = Utils.GetInstance<Structure>(new Regex(@"\.Interfaces\.[^.]*").Replace(flow.Expression, string.Empty));
                    label = Utils.GetLabel(flow.Expression.Split('.').Last());
                }
                
                if(usesInterfaceOwner != null)
                {
                    var flowRelationship = new Relationship(actor.Alias, usesInterfaceOwner.Alias, label);
                    sb.AppendLine(flowRelationship.ToPumlSequenceString(style));
                }

                // TODO: Investigate why doing this in C4 sequence diagrams doesn't seem to work (out of memory java error when rendering)
                if (style == SequenceDiagramStyle.PlantUML)
                {
                    innerFlows.AddRange(flow.GetFlowsByType(flow, Flow.FlowType.Use, true));
                    innerFlows.AddRange(flow.GetFlowsByType(flow, Flow.FlowType.Return, false));
                    innerFlows.AddRange(flow.GetFlowsByType(flow, Flow.FlowType.ThrowException, false));
                    if (innerFlows.Any())
                    {
                        sb.AppendLine($"group {label}");
                    }
                }
            }

            if(flow.Flows != null)
            {
                foreach (var segment in flow.Flows)
                {
                    sb.Append(segment.ToPumlSequenceString(style));
                }
            }

            if (flow.Type == Flow.FlowType.If || 
                flow.Type == Flow.FlowType.Loop || 
                flow.Type == Flow.FlowType.Group)
            {
                sb.AppendLine("end");
            }

            // TODO: Investigate why doing this in C4 sequence diagrams doesn't seem to work (out of memory java error when rendering)
            if (style == SequenceDiagramStyle.PlantUML)
            {
                if (flow.Type == Flow.FlowType.Use && innerFlows.Any())
                {
                    sb.AppendLine("end");
                }
            }

            return sb.ToString();
        }
    }
}
