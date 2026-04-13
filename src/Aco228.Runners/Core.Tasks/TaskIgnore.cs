using Aco228.Common.Extensions;
using Aco228.Common.Models;

namespace Aco228.Runners.Core.Tasks;

public class TaskIgnore
{
    public string Name { get; set; }
    public DateTime IgnoreSetTimeUtc { get; set; } = DateTime.UtcNow;
    public int IgnoreHours { get; set; } = 3;
}

public static class TaskIgnoreExtensions
{
    public static void AddOrRemoveTask(this ConcurrentList<TaskIgnore> list, string taskName)
    {
        if(list.Any(x => x.Name == taskName))
            list.RemoveOne(x => x.Name == taskName);
        else
            list.Add(new()
            {
                Name = taskName,
            });
    }
    
    public static void ValidateIgnoreTasks(this ConcurrentList<TaskIgnore> list)
    {
        foreach (var taskIgnore in list.ToList())
        {
            if (taskIgnore.IgnoreSetTimeUtc.GetHoursDifferenceUTC() < taskIgnore.IgnoreHours)
                continue;

            list.Remove(taskIgnore);
        }
    }
}