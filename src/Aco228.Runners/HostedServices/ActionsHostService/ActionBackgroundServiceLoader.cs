using System.Diagnostics.Tracing;
using Aco228.Common.Extensions;
using Aco228.MongoDb.Extensions;
using Aco228.MongoDb.Models;
using Aco228.Runners.Documents.Actions;
using Aco228.Runners.Extensions;
using Aco228.Runners.HostedServices.ActionsHostService.Models;
using Aco228.Runners.Models;

namespace Aco228.Runners.HostedServices.ActionsHostService;

public class ActionBackgroundServiceLoader
{
    private readonly ActionBackgroundService _service;
    private readonly LoadSpecification<ActionRunDocument, ActionRunDocument> _loadSpec;

    public ActionBackgroundServiceLoader(ActionBackgroundService service)
    {
        _service = service;
        _loadSpec = _service.ActionDocumentRepo.NoTrack()
            .FilterBy(x => ActionStatusExtensions.GetRunnableActions().Contains(x.Status))
            .OrderByPropertyAsc(x => x.LastInteractionUtcTc);
    }
    
    public async Task ReleaseMachineLocks()
    {
        var actions = await _service.ActionDocumentRepo.NoTrack()
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


    internal async Task<List<ActionDefinition>> CollectActionDocument()
    {
        var currentTs = DT.GetUnix();
        var scheduledActions = await _service.ActionDocumentRepo.NoTrack()
            .FilterBy(x => !string.IsNullOrEmpty(x.LockBy) && x.LockBy.Equals(_service.MachineContract.MachineName))
            .FilterBy(x => x.WaitUntilTs == null || x.WaitUntilTs <= currentTs)
            .ToListAsync();
        
        var currentScheduledCount = scheduledActions.Count;
        var result = new List<ActionDefinition>();
        
        foreach (var scheduledAction in scheduledActions)
        {
            if (_service.RunningActions.ContainsCategory(scheduledAction))
                continue;
            
            var definition = new ActionDefinition(scheduledAction);
            currentScheduledCount--;
            if (await definition.HasTypeProblem())
                continue;
            
            result.Add(definition);
        }

        var maximumScheduleActions = ActionBackgroundService.MAXIMUM_ACTIONS_PER_EXECUTION - currentScheduledCount + 2;
        var scheduledCount = 0;
        
        await foreach (var actionDocument in _loadSpec
                           .LoadInBatchesAsync(batchSize: 50, _service.CancellationToken)
                           .WithCancellation(_service.CancellationToken))
        {
            if (actionDocument.TryReleaseLock(ActionBackgroundService.MAXIMUM_EXECUTION_TIME_MIN))
                await _service.ActionDocumentTransactionalManager.InsertOrUpdateAsync(actionDocument);
            
            if (actionDocument.ActionDependencies.Any())
                continue;

            if (_service.RunningActions.ContainsCategory(actionDocument))
                continue;

            if (!actionDocument.CanBeScheduled(_service.MachineContract.MachineName))
                continue;

            await _service.ActionDocumentTransactionalManager.InsertOrUpdateAsync(actionDocument);
            scheduledCount++;
            
            Console.WriteLine($"   >>> Scheduling `{actionDocument.Name}/{actionDocument.Reference}`");

            if (scheduledCount >= maximumScheduleActions)
                break;
        }
        
        await _service.ActionDocumentTransactionalManager.FinishAsync();
        return result;
    }


}