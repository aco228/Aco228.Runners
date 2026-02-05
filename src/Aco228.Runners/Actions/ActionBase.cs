using Aco228.Common.Extensions;
using Aco228.Runners.Actions.ActionManager;
using Aco228.Runners.Actions.Exceptions;
using Aco228.Runners.Documents.Actions;
using Aco228.Runners.Extensions;
using Aco228.Runners.Helpers;

namespace Aco228.Runners.Actions;

public abstract class ActionBase<TRequest, TResponse> : IAction
{
    public string Name { get; set; }
    public virtual string? Category { get; } = null;
    public Type RequestType { get; set; }
    public Type ResponseType { get; set; }

    protected IActionManager ActionManager { get; private set; } = new DefaultActionManager();
    protected virtual ushort MaximumNumberOfErrorRetries { get; } = 1;
    private int ErrorCount { get; set; } = 0;
    protected virtual TimeSpan DelayBetweenRetries { get; } = TimeSpan.FromSeconds(15);
    protected virtual TimeSpan DelayBetweenExecutions { get; } = TimeSpan.FromMinutes(5);

    public Task<TResponse> Execute(TRequest request)
    {
        return GetResponse(request);
    }
    
    internal async Task<TResponse?> GetResponse(TRequest request)
    {
        TResponse? result = default;
        await ActionManager.OnStart();
        for (;;)
        {
            if (ErrorCount >= MaximumNumberOfErrorRetries || ErrorCount > 20)
                break;

            try
            {
                result = await ExecuteInternal(request);
                if (result == null)
                    throw new InvalidOperationException("Action returned null");

                await ActionManager.OnResultReceived(result);
                break;
            }
            catch (ActionDependencyGuardException ex)
            {
                await ActionManager.ChangeStatus(ActionStatus.Waiting, ex.Message);
                break;
            }
            catch (ActionContinueException ex)
            {
                await ActionManager.ChangeStatus(ActionStatus.Waiting, ex.Message);
                break;
            }
            catch (ActionErrorException ex)
            {
                await ActionManager.OnFatalError(ex.Message);
                break;
            }
            catch (Exception ex)
            {
                ErrorCount++;
                if (ErrorCount >= MaximumNumberOfErrorRetries)
                {
                    await ActionManager.OnFatalError($"Exception occurred {ErrorCount} times. " + ex.Message);
                    break;
                }

                await ActionManager.OnError(ex);
            }
            
            await Task.Delay(DelayBetweenRetries);
        }

        var untilNextExecution = DateTime.UtcNow.ToUnixTimestampMilliseconds() + (long)DelayBetweenExecutions.TotalMilliseconds;
        await ActionManager.OnExit(untilNextExecution);
        return result;
    }

    public Task<ActionRunDocument> Schedule(TRequest request, string? reference = null)
        => this.ScheduleInternal(request!, reference);
    
    protected abstract Task<TResponse> ExecuteInternal(TRequest request);
    protected void Log(string message) => ActionManager.Log(message);

    protected T? GetData<T>(string key)
    {
        if(ActionManager is ActionServiceManager manager)
            return manager.GetStoreObject<T>(key);
        return default;
    }

    protected async Task<T?> GetDataOr<T>(string key, Func<Task<T>> task)
    {
        T? result = GetData<T>(key);
        if (result != null)
            return result;
        
        result = await task();
        SetData(key, result);
        return result;
    }
    
    protected void SetData<T>(string key, T value)
    {
        if (ActionManager is ActionServiceManager manager)
            manager.SetStoreObject<T>(key, value);
    }

    protected TRes? RequestAction<TAction, TRes>(object input, string key = null)
        where TAction : IAction
    {
        if (!(ActionManager is ActionServiceManager manager))
            return default;
        
        if(string.IsNullOrEmpty(key))
            key = Name;
        
        return manager.TryGetActionResult<TAction, TRes>(input, key);
    }

    protected void GuardActionResults()
    {
        if (!(ActionManager is ActionServiceManager manager))
            return;

        manager.GuardActionResults();
    }
    
    
    public async Task<object?> ExecuteInBackground(IActionManager actionManager, CancellationToken cancellationToken)
    {
        ActionManager = actionManager;
        var request = actionManager.GetRequestObject().Get<TRequest>();
        if (request == null)
        {
            await actionManager.GetActionDocument().MoveToFailed("Request cannot be unboxed");
            return null;
        }
        
        ErrorCount = ActionManager.GetActionDocument().ErrorCount;
        ActionManager = actionManager;
        return await GetResponse(request).WaitAsync(cancellationToken);
    }
}