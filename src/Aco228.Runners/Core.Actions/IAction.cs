using Aco228.Runners.Documents;
using Aco228.Runners.Models;

namespace Aco228.Runners.Core.Actions;

public enum ActionType
{
    Void,
    Result,
}

public interface IAction
{
    ActionType Type { get; }
    string Name { get; internal set; }
    
    internal Type RequestType { get; set; }
    internal Type ResponseType { get; set; }
}

public abstract class ActionBase : IAction
{
    public abstract ActionType Type { get; }
    public string Name { get; set; }
    public Type RequestType { get; set; }
    public Type ResponseType { get; set; }
    
    public ActionStateManager State { get; internal set; }

    internal abstract Task ExecuteAction(ActionObjectModel request);
    internal virtual object? GetResult() => null;
    
    
}