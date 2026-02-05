using Aco228.Common;
using Aco228.Runners.Core.Tasks;
using Aco228.Runners.Extensions;

namespace Aco228.Runners.Helpers;

public static class RunTasks
{

    public static T Get<T>()
        where T : TaskBase
    {
        var task = ServiceProviderHelper.Construct<T>();
        task.Name = typeof(T).Name;
        return task;
    }

    public static Task Execute<T>(bool forceExecute = true)
        where T : TaskBase
    {
        var task = Get<T>();
        return task.ExecuteTask(name: typeof(T).Name, CancellationToken.None, forceExecute, runAsync: false);
    }
}