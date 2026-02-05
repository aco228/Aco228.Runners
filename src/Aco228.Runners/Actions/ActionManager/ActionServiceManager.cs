using Aco228.Common;
using Aco228.Common.Extensions;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Services;
using Aco228.Runners.Actions.Exceptions;
using Aco228.Runners.Documents;
using Aco228.Runners.Documents.Actions;
using Aco228.Runners.Extensions;
using Aco228.Runners.Helpers;
using Aco228.Runners.HostedServices.ActionsHostService.Models;

namespace Aco228.Runners.Actions.ActionManager;

internal class ActionServiceManager : IActionManager
{
    private record ScheduledAction(Type ActionType, object RequestObject, string Reference);
    
    private readonly ActionDefinition _actionDefinition;
    private IMongoRepo<ActionDataDocument> _actionDataRepo;
    private IMongoRepo<ActionRunDocument> _actionRunDocumentRepo;
    private ActionDataDocument _actionDataDocument;
    private ActionRunDocument _actionDocument => _actionDefinition.Document;
    private List<ScheduledAction> _actionDependenciesForSchedule = new();

    public ActionServiceManager(ActionDefinition actionDefinition)
    {
        _actionDefinition = actionDefinition;
    }

    public async Task Initialize()
    {
        _actionDataRepo = ServiceProviderHelper.GetService<IMongoRepo<ActionDataDocument>>()!;
        _actionRunDocumentRepo = ServiceProviderHelper.GetService<IMongoRepo<ActionRunDocument>>()!;
        _actionDataDocument = await _actionDataRepo.FirstOrDefault(x => x.ActionId == _actionDocument.Id);
        if (_actionDataDocument == null)
            _actionDataDocument = new() { ActionId = _actionDocument.Id, };
    }

    public void Log(string message)
    {
        if(string.IsNullOrEmpty(message)) return;
        _actionDataDocument.Logs.Add(new(message));
        Console.WriteLine(message);
    }
    
    public ActionObjectModel? GetRequestObject() => _actionDocument.Request;
    
    public async Task OnStart()
    {
        Console.WriteLine("Executionstarted::");
        _actionDocument.Status = ActionStatus.Executing;
        _actionDocument.ExecutionStartedTs = DT.GetUnix();
        await _actionRunDocumentRepo.InsertOrUpdateAsync(_actionDocument);
    }

    public ActionRunDocument GetActionDocument() => _actionDocument;

    public async Task ChangeStatus(ActionStatus actionStatus, string statusMessage = "")
    {
        Console.WriteLine($"Status changed to ({actionStatus}) {statusMessage}");
        Log(statusMessage);
        _actionDocument.Status = actionStatus;
        await _actionRunDocumentRepo.InsertOrUpdateAsync(_actionDocument);
    }

    public async Task OnResultReceived(object result)
    {
        if (_actionDocument.ParentId != null && _actionDocument.Reference != null)
        {
            var parentAction = await _actionRunDocumentRepo.FindById(_actionDocument.ParentId.Value);
            if (parentAction != null)
            {
                var parentActionData = await _actionDataRepo.FirstOrDefaultAsync(x => x.ActionId == parentAction.Id);
                if (parentActionData != null)
                {
                    parentActionData.ActionData.AddOrUpdate(_actionDocument.Reference, ActionObjectModelHelper.Get(result));
                    await _actionDataRepo.InsertOrUpdateAsync(parentActionData);
                }
                
                parentAction.ActionDependencies.Remove(_actionDocument.Id);
                await _actionRunDocumentRepo.InsertOrUpdateAsync(parentAction);
            }
        }
        
        _actionDocument.ReleaseLock();
        await _actionDocument.MoveToFinished(result);
    }

    public async Task OnError(Exception ex)
    {
        Console.WriteLine("Exception::" + ex.Message);
        _actionDocument.ErrorCount++;
        await _actionRunDocumentRepo.InsertOrUpdateAsync(_actionDocument);
    }

    public async Task OnFatalError(string errorMessage)
    {
        _actionDocument.ReleaseLock();
        _actionDocument.Status = ActionStatus.Failed;
        Console.WriteLine("FatalException::" + errorMessage);
        await _actionDocument.MoveToFailed(errorMessage);
    }

    public async Task OnExit(long untilNextExecution)
    {
        foreach (var scheduledAction in _actionDependenciesForSchedule)
        {
            Console.WriteLine($"Scheduling action: {scheduledAction.ActionType.Name}");
            var newAction = await Helpers.Actions.ScheduleByTypeInternal(scheduledAction.ActionType, scheduledAction.RequestObject, _actionDocument.Id, scheduledAction.Reference);
            _actionDocument.ActionDependencies.Add(newAction.Id);
        }
        
        if (!_actionDocument.IsCompleted())
        {
            _actionDocument.WaitUntilTs = untilNextExecution;
            _actionDocument.ReleaseLock();
            await _actionRunDocumentRepo.InsertOrUpdateAsync(_actionDocument);
        }
        
        await _actionDataRepo.InsertOrUpdateAsync(_actionDataDocument);
    }
    
    

    public T? GetStoreObject<T>(string key)
    {
        if (_actionDataDocument.Data.TryGetValue(key, out var value))
            return value.Get<T>();
        return default;
    }

    public void SetStoreObject<T>(string key, T value)
    {
        _actionDataDocument.Data[key] = ActionObjectModelHelper.Get(value)!;
    }

    public TResponse? TryGetActionResult<TAction, TResponse>(object request, string reference)
        where TAction : IAction
    {
        var type = typeof(TAction);
        if (type.BaseType == null)
            throw new InvalidOperationException("Action must inherit from IAction");
        
        var genericArguments = type.BaseType.GetGenericArguments();
        if (genericArguments[0] != request.GetType())
            throw new InvalidOperationException("Action request type does not match input type");

        if (string.IsNullOrEmpty(reference))
            reference = type.Name;

        reference = $"{_actionDocument.Id}_{reference}";
        
        if (_actionDataDocument.ActionData.TryGetValue(reference, out var value))
            return value != null && value.Type != typeof(TResponse).FullName ? default : value.Get<TResponse>();
        
        _actionDataDocument.ActionData.Add(reference, value);
        _actionDependenciesForSchedule.Add(new(type, request, reference));
        
        return default;
    }

    public void GuardActionResults()
    {
        if (_actionDataDocument.ActionData.Any(x => x.Value == null))
            throw new ActionDependencyGuardException();
    }
    
}