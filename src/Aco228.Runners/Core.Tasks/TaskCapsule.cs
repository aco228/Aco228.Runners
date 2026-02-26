using System.Reflection.Metadata;
using Aco228.Runners.Extensions;
using Aco228.Runners.Services.Background;
using MongoDB.Bson;

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
        Task.OnCompleted += (sender, args) =>
        {
            _manager.OnTaskFinished(this);
        };
        
        if (!Task.IsTimeOkay())
            return false;
        
        MaximumAllowedExecution = Task.MaximumExecutionAllowed.Add(TimeSpan.FromMinutes(5));
        StartTime = DateTime.Now;

        var prepare = await Task.Prepare();
        if (prepare == false)
        {
            return false;
        }
        
        Execution = Task.ExecuteTask(TaskDefinition.Name, _manager.CancellationToken, forceExecute)
            .WaitAsync(_manager.CancellationToken);

        return true;
    }
}