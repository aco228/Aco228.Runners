using Aco228.Common;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Services;
using Aco228.Runners.Documents.Actions;
using Aco228.Runners.Extensions;
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
    
    public void Log(string message) => _actionDataDocument.Logs.Add(new(message));
    public object GetRequestObject() => _actionDocument.Request;
    
    public async Task OnExecutionStarted()
    {
        _actionDocument.ExecutionStartedTs = DT.GetUnix();
        await _actionRunDocumentRepo.InsertOrUpdateAsync(_actionDocument);
    }

    public ActionRunDocument GetActionDocument() => _actionDocument;

    public async Task ChangeStatus(ActionStatus actionStatus)
    {
        _actionDocument.Status = actionStatus;
        await _actionRunDocumentRepo.InsertOrUpdateAsync(_actionDocument);
    }

    public async Task OnError(Exception ex)
    {
        _actionDocument.ErrorCount++;
        await _actionRunDocumentRepo.InsertOrUpdateAsync(_actionDocument);
    }

    public async Task OnExit()
    {
        await _actionDataRepo.InsertOrUpdateAsync(_actionDataDocument);
    }
}