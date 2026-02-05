using Aco228.Runners.Documents;
using Aco228.Runners.Documents.Actions;

namespace Aco228.Runners.Actions.ActionManager;

public interface IActionManager
{
    void Log(string message);
    
    ActionObjectModel? GetRequestObject();
    ActionRunDocument GetActionDocument(); 
    
    Task OnStart();
    Task ChangeStatus(ActionStatus actionStatus, string statusMessage = "");
    Task OnResultReceived(object result);
    Task OnError(Exception ex);
    Task OnFatalError(string message);
    Task OnExit(long waitUntil);
}

public class DefaultActionManager : IActionManager
{
    public DefaultActionManager() { }
    
    public void Log(string message) => Console.WriteLine(message);

    public ActionObjectModel? GetRequestObject() => null;
    public Task OnStart() => Task.CompletedTask;
    public ActionRunDocument GetActionDocument() => null;
    public async Task ChangeStatus(ActionStatus actionStatus, string statusMessage = "") => Console.WriteLine($"Status changed to ({actionStatus}) {statusMessage}");
    public Task OnResultReceived(object result) => Task.CompletedTask;
    public async Task OnError(Exception ex) => Console.WriteLine("Exception happened:: " + ex);
    public async Task OnFatalError(string errorMessage) => Console.WriteLine("FatalError happened:: " + errorMessage);
    public async Task OnExit(long waitUntil) => Console.WriteLine("Action exit ");
}