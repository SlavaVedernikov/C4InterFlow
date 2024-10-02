using C4InterFlow.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C4InterFlow.Automation
{
    public interface ICodeWriter
    {
        string GetSoftwareSystemCode(string architectureNamespace, string name, string label, string? description = null, string? boundary = null);
        string GetContainerCode(string architectureNamespace, string softwareSystemName, string containerName, string label, string? type = null, string? description = null, string? technology = null, string? boundary = null);
        string GetComponentCode(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string componentType = "None", string? description = null, string? technology = null);
        string GetEntityCode(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string? type = null, string? description = null, string[]? composedOfMany = null, string[]? composedOfOne = null, string? extends = null);
        string GetComponentInterfaceCode(string architectureNamespace, string softwareSystemName, string containerName, string componentName, string name, string label, string? description = null, string? protocol = null, string? path = null, bool? isPrivate = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null);
        string GetContainerInterfaceCode(string architectureNamespace, string softwareSystemName, string containerName, string name, string label, string? description = null, string? protocol = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null);
        string GetSoftwareSystemInterfaceCode(string architectureNamespace, string softwareSystemName, string name, string label, string? description = null, string? protocol = null, string? uses = null, string? input = null, string? inputTemplate = null, string? output = null, string? outputTemplate = null);
        string GetLoopFlowCode(string condition);
        string GetEndLoopFlowCode();
        string GetIfFlowCode(string condition);
        string GetEndIfFlowCode();
        string GetElseIfFlowCode(string condition);
        string GetEndElseIfFlowCode();
        string GetElseFlowCode();
        string GetEndElseFlowCode();
        string GetReturnFlowCode(string? expression = null);
        string GetTryFlowCode();
        string GetEndTryFlowCode();
        string GetCatchFlowCode(string? exception = null);
        string GetEndCatchFlowCode();
        string GetFinallyFlowCode();
        string GetEndFinallyFlowCode();
        string GetThrowExceptionFlowCode(string? exception = null);
        string GetUseFlowCode(string alias);
        string GetActorCode(string architectureNamespace, string type, string name, string label, string? description = null);
        string GetBusinessProcessCode(string architectureNamespace, string name, string label, string businessActivitiesCode, string? description = null);
        string GetActivityCode(string label, string actor, Flow[] flows, string? description = null);
    }
}
