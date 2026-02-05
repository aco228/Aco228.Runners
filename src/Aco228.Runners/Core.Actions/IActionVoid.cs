using Aco228.Runners.Models;
using Aco228.Runners.Models.Actions.Exceptions;

namespace Aco228.Runners.Core.Actions;

public interface IActionVoid<TRequest> : IAction
{
    
}

public abstract class ActionVoidBase<TRequest> : ActionBase, IActionVoid<TRequest>
{
    public override ActionType Type => ActionType.Void;

    internal override Task ExecuteAction(ActionRequestModel request)
    {
        var requestModel = request.GetRequest<TRequest>();
        if (requestModel == null)
            throw new ActionFatalException($"Request model is wrong or missing");
        
        return ExecuteInternal(requestModel);
    }
    
    protected abstract Task ExecuteInternal(TRequest request);
}