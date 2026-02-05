using Aco228.Common.Extensions;

namespace Aco228.Runners.Core.Tasks;

public interface ITask
{
    Task ExecuteTask();
}

public abstract class TaskBase : ITask
{
    protected virtual ushort MaximumNumberOfErrorRetries { get; } = 1;
    private int ErrorCount { get; set; } = 0;
    protected virtual TimeSpan DelayBetweenRetries { get; } = TimeSpan.FromSeconds(15);
    protected virtual TimeSpan DelayBetweenExecutions { get; } = TimeSpan.FromMinutes(5);

    protected abstract Task ExecuteInternal();
    
    public async Task ExecuteTask()
    {
        for (;;)
        {
            if (ErrorCount >= MaximumNumberOfErrorRetries || ErrorCount > 20)
                break;

            try
            {
                await ExecuteInternal();
            }
            catch (Exception ex)
            {
                ErrorCount++;
                if (ErrorCount >= MaximumNumberOfErrorRetries)
                {
                    break;
                }
            }
            
            await Task.Delay(DelayBetweenRetries);
        }

        var untilNextExecution = DateTime.UtcNow.ToUnixTimestampMilliseconds() + (long)DelayBetweenExecutions.TotalMilliseconds;;
    }
}