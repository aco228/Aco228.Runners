using System.Collections.Concurrent;
using Aco228.Common.Extensions;
using Aco228.MongoDb.Extensions;
using Aco228.MongoDb.Extensions.MongoFiltersExtensions;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Services;
using Aco228.Runners.Core;
using Aco228.Runners.Core.Tasks;
using Aco228.Runners.Documents;

namespace Aco228.Runners.Services.Background;

public class TaskManagerService : HostServiceBase
{
    private const int MAXIMUM_PER_TURN = 2;
    
    protected override TimeSpan DelayBetweenRetries => TimeSpan.FromSeconds(15);
    
    private readonly IMongoRepo<TaskDocument> _taskRepo;
    private readonly IHostMachineService _hostMachineService;
    private List<TaskDefinition> Tasks { get; set; } = new();
    internal ConcurrentDictionary<string, TaskCapsule> RunningTasks { get; private set; } = new();
    
    public TaskManagerService(
        IMongoRepo<TaskDocument> taskRepo,
        IHostMachineService hostMachineService)
    {
        _taskRepo = taskRepo;
        _hostMachineService = hostMachineService;
    }
    
    public override async Task Initialize()
    {
        var currentTasks = await _taskRepo.Track().Full().ToListAsync();
        foreach (var taskType in TaskCollection.Tasks)
        {
            var taskDefinition = new TaskDefinition(taskType);
            
            var taskDocument = currentTasks.FirstOrDefault(x => x.Name == taskType.Name);
            if (taskDocument == null)
            {
                taskDocument = new() { Name = taskType.Name };
                currentTasks.Add(taskDocument);
            }

            taskDocument.Delay = taskDocument.Delay;
            taskDocument.From = taskDocument.From;
            taskDocument.To = taskDocument.To;  
            taskDocument.OnlyOnDays = taskDocument.OnlyOnDays;
            taskDefinition.Document = taskDocument;
            Tasks.Add(taskDefinition);
        }
        
        await _taskRepo.InsertOrUpdateManyAsync(currentTasks);
    }

    protected override async Task ExecuteTick()
    {
        Console.WriteLine("Tick");
        if (RunningTasks.Count >= MAXIMUM_PER_TURN)
            return;
        
        var candidates = new List<TaskDefinition>();
        foreach (var taskDefinition in Tasks.OrderByDescending(x => x.Document.LastExecutionUtc))
        {
            if (RunningTasks.Any(x => x.Value.Name == taskDefinition.Name))
                continue;
            
            candidates.Add(taskDefinition);
        }

        if (!candidates.Any())
            return;

        foreach (var candidate in candidates)
        {
            if (RunningTasks.Count >= MAXIMUM_PER_TURN)
                return;
            
            var taskCapsule = new TaskCapsule(this, candidate);
            if (taskCapsule.TryStart(forceExecute: false))
            {
                Console.WriteLine($" ++ adding task {taskCapsule.Name}");
                RunningTasks.TryAdd(taskCapsule.Name, taskCapsule);
            }
        }

    }
    
    internal void OnTaskFinished(TaskCapsule task)
    {
        Console.WriteLine($" -- finished task {task.Name}");
        RunningTasks.WaitRemove(task.Name);
    }
}