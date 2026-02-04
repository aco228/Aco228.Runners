using Aco228.Common;
using Aco228.MongoDb.Extensions;
using Aco228.MongoDb.Services;
using Aco228.Runners.Documents.Actions;
using Aco228.Runners.Documents.Machine;
using Aco228.Runners.Helpers;
using Aco228.Runners.HostedServices.Core;
using Aco228.Runners.Services;

namespace Aco228.Runners.HostedServices.ActionsHostService;

public class ActionBackgroundService : HostServiceBase
{
    internal static TimeSpan ExecuteInterval => TimeSpan.FromSeconds(5);
    internal static ushort MAXIMUM_EXECUTION_TIME_MIN = 15;
    internal static ushort MAXIMUM_ACTIONS_PER_EXECUTION = 10;
    
    internal IMongoRepo<ActionRunDocument> ActionDocumentRepo { get; private set; } 
    internal IMongoRepo<ActionDataDocument> ActionDocumentDataRepo { get; private set; }
    internal HostMachineContract MachineContract { get; private set; }
    internal CancellationToken CancellationToken { get; private set; }
    internal IMongoRepoTransactionalManager<ActionRunDocument> ActionDocumentTransactionalManager { get; private set; }
    private ActionBackgroundServiceLoader _loader;
    private List<ActionStatus> RunnableActionStatues { get; set; }

    public override async Task Initialize()
    {
        Console.WriteLine("Initialize ActionBackgroundService");
        MachineContract = ServiceProviderHelper.GetService<IHostMachineService>()!.GetMachineContract();
        ActionDocumentRepo = ServiceProviderHelper.GetService<IMongoRepo<ActionRunDocument>>()!;
        ActionDocumentDataRepo = ServiceProviderHelper.GetService<IMongoRepo<ActionDataDocument>>()!;
        ActionDocumentTransactionalManager = ActionDocumentRepo.GetTransactionalManager();
        RunnableActionStatues = ActionStatusExtensions.GetRunnableActions();

        _loader = new(this);
        GetExecutableActionsHelper.Initialize(ActionDocumentRepo);
        await _loader.ReleaseMachineLocks();
    }

    protected override async Task ExecuteTick()
    {
        Console.WriteLine("aco");
        await _loader.CollectActionDocument();
    }
}