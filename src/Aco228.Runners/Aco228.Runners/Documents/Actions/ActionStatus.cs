namespace Aco228.Runners.Documents.Actions;

public enum ActionStatus
{
    Scheduled = 1,
    ScheduleLock = 2,
    Finished = 3,
    Executing = 4,
    Failed = 5,
    Waiting = 6,
    ErrorWaiting = 7,
    Canceled = 8,
}

public static class ActionStatusExtensions
{
    public static List<ActionStatus> GetRunnableActions()
        => new()
        {
            ActionStatus.Scheduled,
            ActionStatus.ScheduleLock,
            ActionStatus.Waiting,
            ActionStatus.ErrorWaiting,
            ActionStatus.Executing,
        };
}