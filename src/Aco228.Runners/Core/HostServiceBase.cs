using Aco228.Runners.Helpers.Tasks;
using Microsoft.Extensions.Hosting;

namespace Aco228.Runners.Core;

public abstract class HostServiceBase : BackgroundService
{
    protected virtual string Name => "";
    public CancellationToken CancellationToken { get; private set; }
    protected virtual TimeSpan DelayBetweenRetries { get; } = TimeSpan.FromMinutes(1);
    
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
    
    public virtual void ConsoleLog(string message)
    {
        TaskConsoleHelper.Log($"{Name} | {message}");
    }
}