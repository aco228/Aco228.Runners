using Aco228.Common;
using Aco228.Runners.Core;

namespace Aco228.Runners.Services.Background;

public class HeartbeatBackgroundService : HostServiceBase
{
    private IHostMachineService HostMachineService { get; set; }
    protected override TimeSpan DelayBetweenRetries => TimeSpan.FromMinutes(5);
    
    public override async Task Initialize()
    {
        Console.WriteLine("Initializing HeartbeatBackgroundService");
        HostMachineService = ServiceProviderHelper.GetService<IHostMachineService>()!;
    }

    protected override async Task ExecuteTick()
    {
        Console.WriteLine("Running HeartbeatBackgroundService");
        await HostMachineService.Tick();
    }
}