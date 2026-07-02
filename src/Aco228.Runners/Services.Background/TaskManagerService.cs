using System.Collections.Concurrent;
using Aco228.Common;
using Aco228.Common.Extensions;
using Aco228.Common.Models;
using Aco228.MongoDb.Extensions;
using Aco228.MongoDb.Extensions.MongoFiltersExtensions;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Services;
using Aco228.Runners.Core;
using Aco228.Runners.Core.Tasks;
using Aco228.Runners.Documents;
using Aco228.Runners.Extensions;
using Aco228.Runners.Models.TaskManager;

namespace Aco228.Runners.Services.Background;

public class TaskManagerService : HostServiceBase
{
    private static int MAXIMUM_PER_TURN = 11;
    public static TimeSpan Delay = TimeSpan.FromSeconds(15);
    public static TaskManagerService? Instance { get; private set; }

    public DateTime? PauseUntil { get; set; } = null;
    public bool IsRestartRequested { get; set; } = false;
    protected override TimeSpan DelayBetweenRetries => Delay;

    private bool _isShutdownMode = false;
    private readonly IMongoRepo<TaskDocument> _taskRepo;
    private readonly IHostMachineService _hostMachineService;
    private List<TaskDefinition> Tasks { get; set; } = new();
    public ConcurrentDictionary<string, TaskCapsule> RunningTasks { get; private set; } = new();
    public ConcurrentList<TaskIgnore> TaskIgnores { get; set; } = new();
    
    public TaskManagerService(
        IMongoRepo<TaskDocument> taskRepo,
        IHostMachineService hostMachineService)
    {
        _taskRepo = taskRepo;
        _hostMachineService = hostMachineService;
        Instance = this;
    }
    
    public override async Task Initialize()
    {
        #if DEBUG
        MAXIMUM_PER_TURN = 1;
        #endif

        if (string.IsNullOrEmpty(_hostMachineService.Name))
            throw new InvalidOperationException($"Unknown machine name on TaskManagerInitialize");
        
        var currentTasks = await _taskRepo.Track().Full().Eq(x => x.Owner, _hostMachineService.Name).ToListAsync();
        foreach (var taskType in TaskCollection.Tasks)
        {
            var taskDefinition = new TaskDefinition(taskType);
            var instance = taskDefinition.CreateInstance();
            
            var taskDocument = currentTasks.FirstOrDefault(x => x.Name == taskType.Name);
            if (taskDocument == null)
            {
                taskDocument = new() { Name = taskType.Name, Owner = _hostMachineService.Name };
                currentTasks.Add(taskDocument);
            }

            taskDocument.Delay = instance.Delay;
            taskDocument.DelaySuccess = instance.DelaySuccess;
            taskDocument.From = instance.From;
            taskDocument.To = instance.To;
            taskDocument.OnlyOnDays = instance.OnlyOnDays;
            taskDefinition.Document = taskDocument;
            taskDefinition.PriorityIndex = instance.PriorityIndex;
            Tasks.Add(taskDefinition);
        }
        
        await _taskRepo.InsertOrUpdateManyAsync(currentTasks);
    }

    protected override async Task ExecuteTick()
    {
        if (_isShutdownMode)
        {
            if (RunningTasks.Count > 0)
            {
                Console.WriteLine("--- Under shutdown mode: " + string.Join(", ", RunningTasks.Keys));
                return;
            }
         
            Console.WriteLine("--- Shutdown");
            Environment.Exit(0);
            return;
        }

        if (TaskManagerConstants.IsSyncRequired)
            await TryToSync();
        
        if (PauseUntil != null)
        {
            if (PauseUntil.Value > DateTime.Now)
                return;
            
            PauseUntil = null;   
        }

        TaskIgnores.ValidateIgnoreTasks();
        foreach (var (_, taskCapsule) in RunningTasks)
            await taskCapsule.Validate();
        
        if (RunningTasks.Count >= MAXIMUM_PER_TURN)
        {
            Console.WriteLine("Tick (max)"); 
            return;   
        }
        
        var candidates = new List<TaskDefinition>();
        foreach (var taskDefinition in Tasks.OrderByDescending(x => x.PriorityIndex).ThenByDescending(x => x.Document.LastExecutionUtc))
        {
            if (TaskIgnores.Any(x => x.Name == taskDefinition.Name))
                continue;
            
            if (RunningTasks.Any(x => x.Value.Name == taskDefinition.Name))
                continue;

            if (!string.IsNullOrEmpty(taskDefinition.Category) 
                && RunningTasks.Any(x => x.Value.Category?.Equals(taskDefinition.Category) == true))
                continue;

            if (!taskDefinition.IsTimeOkay())
                continue;
            
            candidates.Add(taskDefinition);
        }

        if (!candidates.Any())
        {
            Console.WriteLine("Tick (no_can)");
            return;
        }

        foreach (var candidate in candidates.OrderByDescending(x => x.PriorityIndex).ThenBy(x => x.Document.LastExecutionUtc))
        {
            if (RunningTasks.Count >= MAXIMUM_PER_TURN)
            {
                Console.WriteLine("Tick (can_max)");
                return;
            }
            
            var taskCapsule = new TaskCapsule(this, candidate);
            var canRun = await taskCapsule.TryStart(forceExecute: false);
            if (canRun)
            {
                Console.WriteLine($" ++ adding task {taskCapsule.Name}");
                RunningTasks.TryAdd(taskCapsule.Name, taskCapsule);
            }
        }
        
        Console.WriteLine("Tick (exe)");
    }

    public void AddOrRemoveIgnoreTask(string taskName) 
        => TaskIgnores.AddOrRemoveTask(taskName);

    public void DemandShutdown()
    {
        Console.WriteLine("[[- TASK MANAGER RECEIVED RESTART");
        _isShutdownMode = true;
    }
    
    internal void OnTaskFinished(TaskCapsule task)
    {
        Console.WriteLine($" -- finished task {task.Name}");
        RunningTasks.WaitRemove(task.Name);
    }

    protected async Task TryToSync()
    {
        if(TaskManagerConstants.IsSyncRequired == false)
            return;
        
        Console.WriteLine($" ||| Try sync");
        
        var provider = ServiceProviderHelper.GetService<ITaskManagerSyncProvider>();
        if (provider == null)
        {
            Console.WriteLine($" ||| Sync provider is null !!!!!");
            return;   
        }

        var sync = await provider.Sync();
        if (sync == null)
        {
            PauseUntil = DateTime.UtcNow.AddMinutes(5);
            Console.WriteLine($" ||| Sync error !!!!!");
            return;
        }
        
        PauseUntil = sync.PausedUntil;
        TaskIgnores = sync.TaskIgnores.ToConcurrentList();
        
        Console.WriteLine($" ||| Sync completed");
    }

    public TaskManagerSyncResponse GetSyncModel()
        => new()
        {
            PausedUntil = PauseUntil,
            TaskIgnores = TaskIgnores.ToList(),
        };
}