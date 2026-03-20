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

public abstract class TaskDateDefinition
{
    public virtual HourWindow From { get; } = HourWindow.DayStart;
    public virtual HourWindow To { get; } = HourWindow.DayEnd;
    public virtual DelayWindow Delay { get; } = new(1, DelayType.Hours);
    public virtual List<DayOfWeek>? OnlyOnDays { get;  } = null;
    public abstract DateTime? LastExecutionInUtc { get; }
}

public abstract class TaskBase : TaskDateDefinition, ITask
{
    public string Name { get; set; }
    public virtual int RunCandidatesInParallelCount { get; set; } = 1;
    public virtual bool PanicOnException { get; } = true;
    public virtual string? Description { get; } = null;
    public virtual string? Category { get; } = null;
    internal TaskDocument? Document { get; set; }
    protected TaskStateMachine StateMachine { get; set; } = new();
    public EventHandler? OnCompleted;
    
    internal ITaskDefinition? TaskDefinition { get; set; } 
    internal virtual bool RunSync { get; } = false;
    internal bool IsPrepared { get; private set; } = false;
    internal DateTime StartTime { get; set; }
    internal virtual TimeSpan MaximumExecutionAllowed { get; } = TimeSpan.FromMinutes(20);
    protected CancellationToken CancellationToken { get; set; }
    public override DateTime? LastExecutionInUtc => TaskDefinition?.LastExecutionInUtc;

    protected abstract Task InternalExecute();
    protected virtual bool CanRun() => true;
    protected virtual Task OnFinish() => Task.FromResult(true);
    
    public Task ExecuteTask(bool forceExecute = false)
        => ExecuteTask(Name, CancellationToken.None, forceExecute);
    
    protected virtual async Task PrepareResources() => await Task.CompletedTask;

    internal async Task<bool> Prepare()
    {
        if(IsPrepared)
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
            
            var canRun = await Prepare();
            var isTimeOk = this.IsTimeOkay();
            if (!forceExecute && (!canRun || !isTimeOk))
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

            IsPrepared = false;
            await StateMachine.Wait();
            Document?.LastCompleteExecutionUtc = DateTime.UtcNow;
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
        
        Document?.LastExecutionUtc = DateTime.UtcNow;

        await OnFinish();
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