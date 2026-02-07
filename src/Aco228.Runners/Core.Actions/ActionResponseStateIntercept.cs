namespace Aco228.Runners.Core.Actions;

public class ActionResultStateIntercept<TRequest, TResponse> : ActionResultBase<TRequest, TResponse>
{
    private readonly ActionStateManager _actionStateManager;

    public ActionResultStateIntercept(ActionStateManager actionStateManager)
    {
        _actionStateManager = actionStateManager;
    }
    
    protected override Task<TResponse?> ExecuteInternal(TRequest request)
    {
        return default;
    }
}