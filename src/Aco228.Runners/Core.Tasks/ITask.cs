using Aco228.Runners.Documents;
using Aco228.Runners.Extensions;
using Aco228.Runners.Helpers.Tasks;
using Aco228.Runners.Models.Actions.Exceptions;
using Aco228.Runners.Models.Timings;

namespace Aco228.Runners.Core.Tasks;

public interface ITask
{
    string Name { get; set; }
    Task ExecuteTask(bool forceExecute = false);
}

public abstract class TaskDateDefinition
{
    public virtual HourWindow From { get; } = HourWindow.DayStart;
    public virtual HourWindow To { get; } = HourWindow.DayEnd;
    public virtual DelayWindow Delay { get; } = new(1, DelayType.Hours);
    public virtual DelayWindow? DelaySuccess { get; } = null;
    public virtual List<DayOfWeek>? OnlyOnDays { get;  } = null;
    public abstract DateTime? LastExecutionInUtc { get; }
    public abstract DateTime? LastSuccessExecutionInUtc { get; }
}

public abstract class TaskBase : TaskDateDefinition, ITask
{
    public string Name { get; set; }
    public virtual int RunCandidatesInParallelCount { get; set; } = 1;
    public virtual bool PanicOnException { get; } = true;
    public virtual string? Description { get; } = null;
    public virtual string? Category { get; } = null;
    public TaskDocument? Document { get; set; }
    protected TaskStateMachine StateMachine { get; set; } = new();
    public EventHandler? OnCompleted;
    public virtual int PriorityIndex { get; } = 0;
    
    internal ITaskDefinition? TaskDefinition { get; set; } 
    internal virtual bool RunSync { get; } = false;
    internal bool IsPrepared { get; set; } = false;
    internal DateTime StartTime { get; set; }
    internal virtual TimeSpan MaximumExecutionAllowed { get; } = TimeSpan.FromMinutes(20);
    protected CancellationToken CancellationToken { get; set; }
    public override DateTime? LastExecutionInUtc => TaskDefinition?.LastExecutionInUtc ?? Document?.LastExecutionUtc;
    public override DateTime? LastSuccessExecutionInUtc => TaskDefinition?.LastSuccessExecutionInUtc ?? Document?.LastSuccessExecutionInUtc;

    protected abstract Task InternalExecute();
    protected virtual bool CanRun() => true;
    internal virtual Task OnFinish() => Task.FromResult(true);
    
    public Task ExecuteTask(bool forceExecute = false)
        => ExecuteTask(Name, CancellationToken.None, forceExecute);
    
    protected virtual async Task PrepareResources() => await Task.CompletedTask;

    internal async Task<bool> Prepare(bool force)
    {
        if(!force && IsPrepared)
            return true;
        
        await PrepareResources();
        IsPrepared = true;
        return CanRun();
    }
    
    internal async Task ExecuteTask(string name, CancellationToken cancellationToken, bool forceExecute = false, bool? runAsync = null)
    {
        Name = name;
        StartTime = DateTime.Now;
        CancellationToken = cancellationToken;

        try
        {
            var runAsyncValue = runAsync ?? RunSync;

            var isTimeOk = this.IsTimeOkay();
            if (!forceExecute && !isTimeOk)
            {
                Document?.LastExecutionUtc = DateTime.UtcNow;
                OnCompleted?.Invoke(this, EventArgs.Empty);
                return;
            }

            var canRun = await Prepare(force: false);
            IsPrepared = false;
            if (!forceExecute && !canRun)
            {
                OnCompleted?.Invoke(this, EventArgs.Empty);
                return;
            }
            
            Document?.LastExecutionUtc = DateTime.UtcNow;
            if (runAsyncValue)
            {
                await InternalExecute();
            }
            else
            {
                await InternalExecute().WaitAsync(MaximumExecutionAllowed, cancellationToken);
            }

            await StateMachine.Wait();
            Document?.LastExecutionUtc = DateTime.UtcNow;
            Document?.LastSuccessExecutionInUtc = DateTime.UtcNow;
        }
        catch (ActionContinueException ex)
        {
            Document?.LastExecutionUtc = DateTime.UtcNow.AddMinutes(-15);
            
        }
        catch (Exception ex)
        {
            if (PanicOnException)
                CaptureError("CriticalTaskException", ex);
            
            ConsoleLog("");
            ConsoleLog("");
            ConsoleLog("");
            ConsoleLog($" [ENGINE]: Error: {ex}");
            ConsoleLog("[ENGINE]: " + ex.StackTrace);
            ConsoleLog("");
            ConsoleLog("");
            ConsoleLog("");
        }
        
        await OnFinish();
        Document?.LastExecutionUtc = DateTime.UtcNow;
        TaskDefinition?.Update();
        OnCompleted?.Invoke(this, EventArgs.Empty);
    }

    protected void CaptureError(string problem, Exception? ex = null, params string[] tags)
    {
        void ConfigureScope(Scope scope)
        {
            scope.Level = SentryLevel.Error;
            scope.SetTag("TaskName", Name);
            scope.SetTag("TaskType", GetType().Name);
            scope.SetExtra("problem", problem);
        
            for (int i = 0; i < tags.Length - 1; i += 2)
                scope.SetTag(tags[i], tags[i + 1]);
        }
        
        ConsoleLog($"ERROR: {problem}, " + ex);

        if (ex == null)
        {
            SentrySdk.CaptureEvent(new SentryEvent
            {
                Message = new SentryMessage { Message = problem },
                Level = SentryLevel.Error,
            }, ConfigureScope);
        }
        else
        {
            SentrySdk.CaptureException(ex, ConfigureScope);
        }
        
        _ = Task.Run(() => SentrySdk.FlushAsync(TimeSpan.FromSeconds(3)));
    }
    
    public virtual void ConsoleLog(string message)
    {
        TaskConsoleHelper.Log($"{Name} | {message}");
    }
}