using Aco228.Common;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Services;
using Aco228.Runners.Documents;
using Aco228.Runners.Documents.Actions;
using Aco228.Runners.Extensions;
using Aco228.Runners.Helpers;
using Aco228.Runners.HostedServices.ActionsHostService.Models;

namespace Aco228.Runners.Actions.ActionManager;

internal class BackgroundServiceActionManager : IActionManager
{
    private readonly ActionDefinition _actionDefinition;
    private IMongoRepo<ActionDataDocument> _actionDataRepo;
    private IMongoRepo<ActionRunDocument> _actionRunDocumentRepo;
    private ActionDataDocument _actionDataDocument;
    private ActionRunDocument _actionDocument => _actionDefinition.Document;

    public BackgroundServiceActionManager(ActionDefinition actionDefinition)
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

    public async Task OnExit(long untilNextExecution)
    {
        if (!_actionDocument.IsCompleted())
        {
            _actionDocument.WaitUntilTs = untilNextExecution;
            _actionDocument.ReleaseLock();
            await _actionRunDocumentRepo.InsertOrUpdateAsync(_actionDocument);
        }
        
        await _actionDataRepo.InsertOrUpdateAsync(_actionDataDocument);
    }
}