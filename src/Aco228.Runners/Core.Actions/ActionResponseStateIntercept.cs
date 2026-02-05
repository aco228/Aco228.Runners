namespace Aco228.Runners.Core.Actions;

public class ActionResponseStateIntercept<TRequest, TResponse> : ActionResponseBase<TRequest, TResponse>
{
    private readonly ActionStateManager _actionStateManager;

    public ActionResponseStateIntercept(ActionStateManager actionStateManager)
    {
        _actionStateManager = actionStateManager;
    }
    
    protected override Task<TResponse?> ExecuteInternal(TRequest request)
    {
        return default;
    }
}