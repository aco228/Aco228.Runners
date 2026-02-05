namespace Aco228.Runners.UnitOfWork;

public interface IValueWork<TReq, TRes> : IUnitOfWork
{
    Task<TRes?> GetResult(TReq request);
}

public abstract class ValueWork<TReq, TRes> : UnitOfWork, IValueWork<TReq, TRes>
{
    public string Name { get; }
    public TRes? Result { get; set; }
    
    
    public async Task<TRes?> GetResult(TReq request)
    {
        Result = await WrapValueLogic(() => Execute(request));
        return Result;
    }
    
    protected abstract Task<TRes> Execute(TReq request);
}
