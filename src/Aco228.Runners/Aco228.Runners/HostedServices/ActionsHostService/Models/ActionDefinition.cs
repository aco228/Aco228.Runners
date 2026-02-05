using Aco228.Runners.Actions.ActionManager;
using Aco228.Runners.Documents.Actions;
using Aco228.Runners.Extensions;
using Aco228.Runners.Models;

namespace Aco228.Runners.HostedServices.ActionsHostService.Models;

internal class ActionDefinition
{
    public ActionRunDocument Document { get; private set; }
    public string Name => Document.Name;
    public Type ActionType { get; private set; }
    public Type RequestType { get; private set; }
    public Type ResponseType { get; private set; }
    private Task? RunningTask { get; set; }
    private RunningActionCollection RunningActionCollection { get; set; }
    
    public bool IsRunning => RunningTask?.Status is TaskStatus.Running or TaskStatus.WaitingForActivation;

    public ActionDefinition(ActionRunDocument document)
    {
        Document = document;
    }

    public async Task<bool> HasTypeProblem()
    {
        if(!ActionAssembliesData.TryGetType(Document.TypeDescription, out var type)
           || !ActionAssembliesData.TryGetType(Document.RequestType, out var requestType)
           || !ActionAssembliesData.TryGetType(Document.ResponseType, out var responseType))
        {
            Document.Status = ActionStatus.Failed;
            await Document.MoveToFailed("Fatal. Type not found");
            return true;
        }
        
        ActionType = type;
        RequestType = requestType;
        ResponseType = responseType;
        
        return false;
    }

    public void Start(CancellationToken cancellationToken, RunningActionCollection runningActions)
    {
        RunningActionCollection = runningActions;
        RunningTask = Execute(cancellationToken).WaitAsync(cancellationToken);
        RunningActionCollection.Add(this);
    }

    private async Task Execute(CancellationToken cancellationToken)
    {
        try
        {
            var action = Helpers.Actions.GetByType(ActionType);
            var backgroundServiceManager = new BackgroundServiceActionManager(this);
            await backgroundServiceManager.Initialize();
            var result = await action.ExecuteInBackground(backgroundServiceManager, cancellationToken);
        }
        finally
        {
            RunningActionCollection.Remove(this);
        }
    }
    
}