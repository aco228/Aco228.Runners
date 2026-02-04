using Microsoft.Extensions.Hosting;

namespace Aco228.Runners.HostedServices.Core;

public abstract class HostServiceBase : BackgroundService
{
    protected CancellationToken CancellationToken { get; private set; }
    protected virtual TimeSpan DelayBetweenRetries => TimeSpan.FromSeconds(1);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        CancellationToken = stoppingToken;
        
        for(;;)
        {
            await ExecuteTick();
            await Task.Delay(DelayBetweenRetries, stoppingToken);
        }
    }
    
    public abstract Task Initialize();
    protected abstract Task ExecuteTick();
}