using Aco228.Common;
using Aco228.Common.Extensions;
using Aco228.MongoDb.Extensions;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Services;
using Aco228.Runners.Documents.Actions;
using Aco228.Runners.Documents.Machine;
using Aco228.Runners.Extensions;
using Aco228.Runners.HostedServices.ActionsHostService;
using Aco228.Runners.Services;

namespace Aco228.Runners.Helpers;

internal static class GetExecutableActionsHelper
{
    public static List<ActionStatus> StatusCandidates = new()
    {
        ActionStatus.Scheduled,
        ActionStatus.Waiting,
        ActionStatus.ScheduleLock,
        ActionStatus.Executing,
        ActionStatus.ErrorWaiting,
    };
    private static IMongoRepo<ActionRunDocument> _actionDocumentRepo;
    private static IMongoRepoTransactionalManager<ActionRunDocument> _actionDocumentTransactionalManager;
    private static LoadSpecification<ActionRunDocument, ActionRunDocument> _loadSpec;
    private static HostMachineContract _machineContract;

    public static void Initialize(IMongoRepo<ActionRunDocument> repo)
    {
        _machineContract = ServiceProviderHelper.GetService<IHostMachineService>()!.GetMachineContract();
        _actionDocumentRepo = repo;
        _actionDocumentTransactionalManager = _actionDocumentRepo.GetTransactionalManager().SetLimit(5);
        _loadSpec = _actionDocumentRepo.Load()
            .FilterBy(x => StatusCandidates.Contains(x.Status))
            .OrderByPropertyAsc(x => x.LastInteractionUtcTc);
    }

    public static async Task ReleaseMachineLocks()
    {
        var actions = await _actionDocumentRepo.Load()
            .FilterBy(x => x.LockBy == _machineContract.MachineName)
            .ToListAsync();

        foreach (var actionDocument in actions)
        {
            actionDocument.ReleaseLock();
            await _actionDocumentTransactionalManager.InsertOrUpdateAsync(actionDocument);
        }

        await _actionDocumentTransactionalManager.FinishAsync();
    }
    
    public static async IAsyncEnumerable<ActionRunDocument> CollectActionDocument(CancellationToken cancellationToken)
    {
        
        await foreach (var actionDocument in _loadSpec
                           .LoadInBatchesAsync(batchSize: 50, cancellationToken)
                           .WithCancellation(cancellationToken))
        {
            if(actionDocument.LockTimeTs.GetMinutesDifferenceUTC() > (ActionBackgroundService.MAXIMUM_EXECUTION_TIME_MIN + 20))
                actionDocument.ReleaseLock();
            
            if(actionDocument.ExecutionStartedTs.GetMinutesDifferenceUTC() > ActionBackgroundService.MAXIMUM_EXECUTION_TIME_MIN + 10)
                actionDocument.ReleaseLock();
            
            
                
            if (actionDocument.Status == ActionStatus.ScheduleLock)
            {
                if (!string.IsNullOrEmpty(actionDocument.LockBy))
                {
                    
                }
                
                actionDocument.Status = ActionStatus.Scheduled;
            }
            
            yield return actionDocument;
        }
    }
}