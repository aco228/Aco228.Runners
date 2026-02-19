using Aco228.Common;
using Aco228.Runners.Core.Tasks;

namespace Aco228.Runners.Helpers.Tasks;

public static class TaskHelper
{
    public static T Get<T>()
        where T : ITask
    {
        var task = ServiceProviderHelper.Construct<T>();
        task.Name = typeof(T).Name;
        return task;
    }
    
}