using Aco228.Runners.Extensions;
using Aco228.Runners.Services.Background;

namespace Aco228.Runners.Core.Tasks;

internal class TaskCapsule
{
    private readonly TaskManagerService _manager;
    public string Name => TaskDefinition.Name;
    public DateTime StartTime { get; private set; }
    public TaskBase Task { get; private set; }
    public TaskDefinition TaskDefinition { get; private set; }
    public Task Execution { get; private set; }
    public TimeSpan MaximumAllowedExecution { get; private set; }
    
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
        StartTime = DateTime.Now;

        try
        {
            var isReady = await Task.Prepare();
            if (isReady == false)
            {
                return false;
            }

            Task.OnCompleted += (sender, args) => { _manager.OnTaskFinished(this); };

            Execution = Task
                .ExecuteTask(TaskDefinition.Name, _manager.CancellationToken, forceExecute)
                .WaitAsync(_manager.CancellationToken);

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
}