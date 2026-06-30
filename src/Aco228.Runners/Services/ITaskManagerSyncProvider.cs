using Aco228.Runners.Models.TaskManager;

namespace Aco228.Runners.Services;

public interface ITaskManagerSyncProvider
{
    Task<TaskManagerSyncResponse?> Sync();
}