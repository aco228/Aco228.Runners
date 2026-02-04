using Aco228.Runners.Actions.Exceptions;
using Aco228.Runners.Documents.Actions;
using Aco228.Runners.Helpers;

namespace Aco228.Runners.Actions;

public abstract class ActionBase<TRequest, TResponse> : IAction
{
    public string Name { get; set; }
    public Type RequestType { get; set; }
    public Type ResponseType { get; set; }
    protected virtual ushort NumbersOfRetries { get; } = 1;
    private int NumberOfRuns { get; set; } = 0;
    protected virtual TimeSpan DelayBetweenRetries { get; } = TimeSpan.FromSeconds(15);

    public Task<TResponse> Execute(TRequest request)
    {
        return GetResponse(request);
    }
    
    internal async Task<TResponse> GetResponse(TRequest request)
    {
        for (;;)
        {
            if(NumberOfRuns >= NumbersOfRetries
               || NumberOfRuns > 20)
                break;
            
            try
            {
                var result = await ExecuteInternal(request);
                if (result != null)
                    return result;
            }
            catch (ActionContinueException) { }
            catch(Exception ex)
            {
                NumberOfRuns++;
                int a = 0;
            }
            
            await Task.Delay(DelayBetweenRetries);
        }
        
        return default;
    }

    public Task<ActionRunDocument> Schedule(TRequest request, string? reference = null)
        => this.ScheduleInternal(request!, reference);
    
    protected abstract Task<TResponse> ExecuteInternal(TRequest request);
}