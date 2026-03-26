using Aco228.Runners.Extensions;
using Aco228.Runners.Services.Background;

namespace Aco228.Runners.Core.Tasks;

public class TaskCapsule
{
    private readonly TaskManagerService _manager;
    public string Name => TaskDefinition.Name;
    public string? Category => TaskDefinition.Category;
    public DateTime StartTime { get; private set; } = DateTime.Now;
    public TaskBase Task { get; private set; }
    public TaskDefinition TaskDefinition { get; private set; }
    public Task Execution { get; private set; }
    public TimeSpan MaximumAllowedExecution { get; private set; }
    public CancellationTokenSource CancellationToken { get; set; } = new();
    
    public TaskCapsule(TaskManagerService manager, TaskDefinition taskDefinition)
    {
        _manager = manager;
        TaskDefinition = taskDefinition;
    }

    public async Task<bool> TryStart(bool forceExecute)
    {
        Task = TaskDefinition.Initialize();
        if (!Task.IsTimeOkay())
            return false;

        MaximumAllowedExecution = Task.MaximumExecutionAllowed.Add(TimeSpan.FromMinutes(5));

        try
        {
            var isReady = await Task.Prepare(force: true);
            if (isReady == false)
            {
                Task.IsPrepared = false;
                return false;
            }

            StartTime = DateTime.Now;
            Task.OnCompleted += (sender, args) => { _manager.OnTaskFinished(this); };

            Execution = Task
                .ExecuteTask(TaskDefinition.Name, CancellationToken.Token, forceExecute)
                .WaitAsync(CancellationToken.Token);

            return true;
        }
        catch (Exception ex)
        {
            SentrySdk.CaptureException(ex, scope =>
            {
                scope.Level = SentryLevel.Error;
                scope.SetTag("Location", "TaskCapsule");
            });
            return false;
        }
    }

    public async Task Validate()
    {
        if (Execution.IsCompleted)
        {
            _manager.OnTaskFinished(this);
            return;
        }

        if (DateTime.Now - StartTime > MaximumAllowedExecution)
        {
            await Task.OnFinish();
            await CancellationToken.CancelAsync();
        }
    }
}