using Aco228.Runners.Actions.ActionManager;

namespace Aco228.Runners.Actions;

public interface IAction
{
    string Name { get; internal set; }
    string? Category { get; }
    internal Type RequestType { get; set; }
    internal Type ResponseType { get; set; }

    internal Task<object?> ExecuteInBackground(IActionManager actionManager);
}
