using Aco228.Runners.Models;
using Aco228.Runners.Models.Actions;
using Aco228.Runners.Models.Actions.Exceptions;

namespace Aco228.Runners.Core.Actions;

public interface IActionResult<TRequest, TResponse> : IAction
{
    
}

public abstract class ActionResponseBase<TRequest, TResponse> : ActionBase, IActionResult<TRequest, TResponse>
{
    public override ActionType Type => ActionType.Result;
    private TResponse? _result;
    
    internal override object? GetResult() => _result;
    protected abstract Task<TResponse?> ExecuteInternal(TRequest request);

    internal override async Task ExecuteAction(ActionRequestModel request)
    {
        var requestModel = request.GetRequest<TRequest>();
        if(requestModel == null)
            throw new ActionFatalException($"Request model is wrong or missing");
        
        _result = await ExecuteInternal(requestModel);
    }
    
    public Task<TResponse?> GetResultAsync(TRequest request) => ExecuteInternal(request);

    public async Task<ActionPromise> GetPromiseFor(TRequest request)
    {
        return null;
    }
    
    
}