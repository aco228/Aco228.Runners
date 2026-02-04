using Aco228.Common;
using Aco228.MongoDb.Services;
using Aco228.Runners.Documents.Actions;
using Aco228.Runners.Documents.Machine;
using Aco228.Runners.Helpers;
using Aco228.Runners.HostedServices.Core;
using Aco228.Runners.Services;

namespace Aco228.Runners.HostedServices.ActionsHostService;

public class ActionBackgroundService : HostServiceBase
{
    internal static TimeSpan ExecuteInterval => TimeSpan.FromSeconds(15);
    internal static ushort MAXIMUM_EXECUTION_TIME_MIN = 15;
    internal static ushort MAXIMUM_ACTIONS_PER_EXECUTION = 1;
    
    internal IMongoRepo<ActionRunDocument> ActionDocumentRepo { get; private set; }
    internal HostMachineContract MachineContract { get; private set; }
    internal CancellationToken CancellationToken { get; private set; }
    internal IMongoRepoTransactionalManager<ActionRunDocument> ActionDocumentTransactionalManager { get; private set; }
    internal RunningActionCollection RunningActions { get; set; } = new();
    private ActionBackgroundServiceLoader _loader;
    internal List<ActionStatus> RunnableActionStatues { get; set; }

    public override async Task Initialize()
    {
        Console.WriteLine("Initialize ActionBackgroundService");
        MachineContract = ServiceProviderHelper.GetService<IHostMachineService>()!.GetMachineContract();
        ActionDocumentRepo = ServiceProviderHelper.GetService<IMongoRepo<ActionRunDocument>>()!;
        ActionDocumentTransactionalManager = ActionDocumentRepo.GetTransactionalManager();
        RunnableActionStatues = ActionStatusExtensions.GetRunnableActions();

        _loader = new(this);
        GetExecutableActionsHelper.Initialize(ActionDocumentRepo);
        await _loader.ReleaseMachineLocks();
    }

    protected override async Task ExecuteTick()
    {
        Console.WriteLine($"ActionBackgroundService.ExecuteTick (running actions: {RunningActions.Count})");
        var runningCount = RunningActions.Count;
        
        if(runningCount >= MAXIMUM_ACTIONS_PER_EXECUTION)
            return;

        await foreach (var actionDefinition in _loader.CollectActionDocument().WithCancellation(CancellationToken))
        {
            actionDefinition.TryStart(CancellationToken, RunningActions);
            Console.WriteLine($"   >>> Starting action: {actionDefinition.Name}");
            if(RunningActions.Count == runningCount)
                break;
        }
        
        await ActionDocumentTransactionalManager.FinishAsync();
    }
}