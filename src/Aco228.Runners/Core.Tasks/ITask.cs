using Aco228.Runners.Documents;
using Aco228.Runners.Extensions;
using Aco228.Runners.Helpers.Tasks;
using Aco228.Runners.Models.Timings;

namespace Aco228.Runners.Core.Tasks;

public interface ITask
{
    string Name { get; set; }
    Task ExecuteTask(bool forceExecute = false);
}

public abstract class TaskBase : ITask
{
    public string Name { get; set; }
    public virtual int RunCandidatesInParallelCount { get; set; } = 1;
    public virtual string? Description { get; } = null;
    public virtual HourWindow From { get; } = HourWindow.DayStart;
    public virtual HourWindow To { get; } = HourWindow.DayEnd;
    public virtual DelayWindow Delay { get; } = new(1, DelayType.Hours);
    public virtual List<DayOfWeek>? OnlyOnDays { get;  } = null;
    public virtual string? Category { get; } = null;
    internal TaskDocument? Document { get; set; }
    protected TaskStateMachine StateMachine { get; set; } = new();
    public EventHandler? OnCompleted;
    
    internal ITaskDefinition? TaskDefinition { get; set; } 
    internal virtual bool RunSync { get; } = false;
    internal DateTime StartTime { get; set; }
    internal bool IsRunning { get; set; } = false;
    internal virtual TimeSpan MaximumExecutionAllowed { get; } = TimeSpan.FromMinutes(20);
    protected CancellationToken CancellationToken { get; set; }
    public DateTime? LastExecutionInUtc => TaskDefinition?.LastExecutionInUtc;

    protected abstract Task InternalExecute();
    protected virtual bool CanRun() => true;
    protected virtual Task OnFinish() => Task.FromResult(true);
    
    public Task ExecuteTask(bool forceExecute = false)
        => ExecuteTask(Name, CancellationToken.None, forceExecute);
    
    protected virtual async Task PrepareResources() => await Task.CompletedTask;
    
    internal async Task ExecuteTask(string name, CancellationToken cancellationToken, bool forceExecute = false, bool? runAsync = null)
    {
        Name = name;
        StartTime = DateTime.Now;
        CancellationToken = cancellationToken;
        
        if (CanRun() && (!forceExecute && !this.IsTimeOkay()) || IsRunning)
        {
            OnCompleted.Invoke(this, EventArgs.Empty);
            return;
        }

        try
        {
            Document?.LastExecutionUtc = DateTime.UtcNow;
            IsRunning = true;
            var runAsyncValue = runAsync ?? RunSync;
            
            await PrepareResources();

            if (runAsyncValue)
            {
                await InternalExecute();
            }
            else
            {
                await InternalExecute().WaitAsync(MaximumExecutionAllowed, cancellationToken);
            }
            
            await StateMachine.Wait();
            Document?.LastCompleteExecutionUtc = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            ConsoleLog("");
            ConsoleLog("");
            ConsoleLog("");
            ConsoleLog($" [ENGINE]: Error: {ex}");
            ConsoleLog("[ENGINE]: " + ex.StackTrace);
            ConsoleLog("");
            ConsoleLog("");
            ConsoleLog("");
        }
        
        Document?.LastExecutionUtc = DateTime.UtcNow;

        await OnFinish();
        TaskDefinition?.Update();
        OnCompleted?.Invoke(this, EventArgs.Empty);
    }
    
    public virtual void ConsoleLog(string message)
    {
        TaskConsoleHelper.Log($"{Name} | {message}");
    }
}