using Aco228.Runners.Helpers.Tasks;
using Microsoft.Extensions.Hosting;

namespace Aco228.Runners.Core;

public abstract class HostServiceBase : BackgroundService
{
    protected virtual string Name => "";
    public CancellationToken CancellationToken { get; private set; }
    protected virtual TimeSpan DelayBetweenRetries { get; } = TimeSpan.FromMinutes(1);
    private bool _isInitialized = false;
    private SemaphoreSlim _initializeLock = new(1, 1);
    
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        CancellationToken = stoppingToken;
        await Initialize();
        
        for(;;)
        {
            try
            {
                await ExecuteTick();
                await Task.Delay(DelayBetweenRetries, stoppingToken);
            }
            catch
            {
                var nextDelay = DelayBetweenRetries * 2;
                await Task.Delay(nextDelay, stoppingToken);
            }
        }
    }

    public async Task Initialize()
    {
        await _initializeLock.WaitAsync(CancellationToken);
        if (_isInitialized)
        {
            _initializeLock.Release();
            return;
        }
        
        try
        {
            await InitializeInternal();
            _isInitialized = true;
        }
        finally
        {
            _initializeLock.Release();
        }
    }
    
    protected abstract Task InitializeInternal();
    protected abstract Task ExecuteTick();

    public virtual void ConsoleLog(string message)
    {
        TaskConsoleHelper.Log($"{Name} | {message}");
    }
}