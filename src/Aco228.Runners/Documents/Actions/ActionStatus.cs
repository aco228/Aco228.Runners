namespace Aco228.Runners.Documents.Actions;

public enum ActionStatus
{
    Waiting = 1,
    Executing = 2,
    Finished = 3,
    Failed = 4 ,
    Canceled = 5,
    
}

public static class ActionStatusExtensions
{
    public static List<ActionStatus> GetRunnableActions()
        => new()
        {
            ActionStatus.Waiting,
            ActionStatus.Executing,
        };
}