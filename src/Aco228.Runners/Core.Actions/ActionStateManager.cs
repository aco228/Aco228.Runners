namespace Aco228.Runners.Core.Actions;

public class ActionStateManager
{
    public virtual async Task<T?> GetFromStore<T>(string key)
    {
        return default;
    }
    
    public virtual async Task SetToStore<T>(string key, T value) { }
    
    public async Task OnStart() => await Task.CompletedTask;
    public async Task OnFatal(Exception ex) => await Task.CompletedTask;
    public async Task OnExit() => await Task.CompletedTask;
}