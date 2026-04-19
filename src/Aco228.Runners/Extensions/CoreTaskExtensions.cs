using Aco228.Runners.Core.Tasks;
using Aco228.Runners.Models.Timings;

namespace Aco228.Runners.Extensions;

public static class CoreTaskExtensions
{
    public static async Task ExecuteByName<T>(this T task, string name, bool forceExecute = false, CancellationToken? cancellationToken = null)
        where T : TaskBase
    {
        if (cancellationToken == null)
            cancellationToken = new CancellationTokenSource().Token;
        
        task.ConsoleLog($" [ENGINE]: Executing {name}");
        await task.ExecuteTask(name, cancellationToken.Value, forceExecute);
        task.ConsoleLog($" [ENGINE]: Finished {name}");
    }

    public static T SetParallelCount<T>(this T task, int count) where T : TaskBase
    {
        task.RunCandidatesInParallelCount = count;
        return task;
    }
    
    public static bool IsTimeOkay(this TaskDateDefinition task)
    {
        var currentTime = DateTime.Now;

        var isTimeOkay = HourWindowExtensions.IsTimeOkay(task.From, task.To); 
        if (!isTimeOkay)
            return false;
        
        if (task.OnlyOnDays != null && task.OnlyOnDays.Any() && !task.OnlyOnDays.Contains(currentTime.DayOfWeek))
            return false;

        if (task.LastSuccessExecutionInUtc != null && task.DelaySuccess != null)
        {
            var currentUtcTime = DateTime.UtcNow;
            var difference = 0.0;
            
            if (task.DelaySuccess.Type == DelayType.Days)
                difference = (currentUtcTime - task.LastSuccessExecutionInUtc.Value).TotalDays;
            if (task.DelaySuccess.Type == DelayType.Hours)
                difference = (currentUtcTime - task.LastSuccessExecutionInUtc.Value).TotalHours;
            if (task.DelaySuccess.Type == DelayType.Minutes)
                difference = (currentUtcTime - task.LastSuccessExecutionInUtc.Value).TotalMinutes;
            if (task.DelaySuccess.Type == DelayType.Seconds)
                difference = (currentUtcTime - task.LastSuccessExecutionInUtc.Value).TotalSeconds;
            
            if (difference < task.DelaySuccess.Value)
                return false;
        }
        
        if (task.LastExecutionInUtc != null)
        {
            var currentUtcTime = DateTime.UtcNow;
            var difference = 0.0;
            
            if (task.Delay.Type == DelayType.Days)
                difference = (currentUtcTime - task.LastExecutionInUtc.Value).TotalDays;
            if (task.Delay.Type == DelayType.Hours)
                difference = (currentUtcTime - task.LastExecutionInUtc.Value).TotalHours;
            if (task.Delay.Type == DelayType.Minutes)
                difference = (currentUtcTime - task.LastExecutionInUtc.Value).TotalMinutes;
            if (task.Delay.Type == DelayType.Seconds)
                difference = (currentUtcTime - task.LastExecutionInUtc.Value).TotalSeconds;
            
            if (difference < task.Delay.Value)
                return false;
        }

        return true;
    }
}