using Aco228.Common.Models;
using Aco228.Runners.Documents.Actions;
using Aco228.Runners.HostedServices.ActionsHostService.Models;

namespace Aco228.Runners.HostedServices.ActionsHostService;

internal class RunningActionCollection : ConcurrentList<ActionDefinition>
{
    public int RunningCount => this.Count(x => x.IsRunning);


    public bool ContainsCategory(ActionRunDocument document)
    {
        if (string.IsNullOrEmpty(document.ActionCategory))
            return false;
        
        return this.Any(x => x.Document.ActionCategory == document.ActionCategory);
    }
    
}