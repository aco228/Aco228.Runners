using Aco228.Runners.Core.Tasks;

namespace Aco228.Runners.Models.TaskManager;

public class TaskManagerSyncResponse
{
    public DateTime? PausedUntil { get; set; } = null;
    public List<TaskIgnore> TaskIgnores { get; set; } = new();
}