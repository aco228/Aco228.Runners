using Aco228.Runners.Documents.Actions;

namespace Aco228.Runners.Actions.ActionManager;

public interface IActionManager
{
    void Log(string message);
    object GetRequestObject();
    Task OnExecutionStarted();
    ActionRunDocument GetActionDocument(); 
    Task ChangeStatus(ActionStatus actionStatus);
    Task OnError(Exception ex);
    Task OnExit();
}

public class DefaultActionManager : IActionManager
{
    public DefaultActionManager() { }
    
    public void Log(string message) => Console.WriteLine(message);

    public object GetRequestObject() => null;
    public Task OnExecutionStarted() => Task.CompletedTask;
    public ActionRunDocument GetActionDocument() => null;
    public async Task ChangeStatus(ActionStatus actionStatus) => Console.WriteLine("Status changed to " + actionStatus);
    public async Task OnError(Exception ex) => Console.WriteLine("Exception happened:: " + ex);
    public async Task OnExit() => Console.WriteLine("Action exit ");
}