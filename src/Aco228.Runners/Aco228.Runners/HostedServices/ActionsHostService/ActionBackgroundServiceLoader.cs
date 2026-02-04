using System.Diagnostics.Tracing;
using Aco228.Common.Extensions;
using Aco228.MongoDb.Extensions;
using Aco228.MongoDb.Models;
using Aco228.Runners.Documents.Actions;
using Aco228.Runners.Extensions;
using Aco228.Runners.Models;

namespace Aco228.Runners.HostedServices.ActionsHostService;

public class ActionBackgroundServiceLoader
{
    private readonly ActionBackgroundService _service;
    private readonly LoadSpecification<ActionRunDocument, ActionRunDocument> _loadSpec;

    public ActionBackgroundServiceLoader(ActionBackgroundService service)
    {
        _service = service;
        _loadSpec = _service.ActionDocumentRepo.Load()
            .FilterBy(x => ActionStatusExtensions.GetRunnableActions().Contains(x.Status))
            .FilterBy(x => x.IsContextGroup)
            .OrderByPropertyAsc(x => x.LastInteractionUtcTc);
    }
    
    public async Task ReleaseMachineLocks()
    {
        var actions = await _service.ActionDocumentRepo.Load()
            .FilterBy(x => x.LockBy == _service.MachineContract.MachineName)
            .ToListAsync();

        foreach (var actionDocument in actions)
        {
            // if its less then 5min, its okay
            if (actionDocument.LockTimeTs.GetMinutesDifferenceUTC() < 5)
                continue;
            
            actionDocument.ReleaseLock();
            await _service.ActionDocumentTransactionalManager.InsertOrUpdateAsync(actionDocument);
        }

        await _service.ActionDocumentTransactionalManager.FinishAsync();
    }


    public async Task CollectActionDocument()
    {
        int scheduledCount = 0;

        var scheduledActions = await _service.ActionDocumentRepo.Load()
            .FilterBy(x => !string.IsNullOrEmpty(x.LockBy) && x.LockBy.Equals(_service.MachineContract.MachineName))
            .ToListAsync();
        
        foreach (var scheduledAction in scheduledActions)
        {
            if(!ActionAssembliesData.TryGetType(scheduledAction.TypeDescription, out var type))
            {
                scheduledAction.Status = ActionStatus.Failed;
                await scheduledAction.SetErrorWithMessage("Fatal. Type not found");
                continue;
            }
            
            Console.WriteLine("ScheduledType=" + type.FullName);
        }


        await foreach (var actionDocument in _loadSpec
                           .LoadInBatchesAsync(batchSize: 50, _service.CancellationToken)
                           .WithCancellation(_service.CancellationToken))
        {
            if (actionDocument.TryReleaseLock(ActionBackgroundService.MAXIMUM_EXECUTION_TIME_MIN))
                await _service.ActionDocumentTransactionalManager.InsertOrUpdateAsync(actionDocument);

            if (!actionDocument.CanBeScheduled(_service.MachineContract.MachineName))
                continue;

            await _service.ActionDocumentTransactionalManager.InsertOrUpdateAsync(actionDocument);
            scheduledCount++;
            
            Console.WriteLine($"Scheduling `{actionDocument.Name}` to machine `{_service.MachineContract.MachineName}`");

            if (scheduledCount >= ActionBackgroundService.MAXIMUM_ACTIONS_PER_EXECUTION)
                break;
        }
        
        await _service.ActionDocumentTransactionalManager.FinishAsync();
    }


}