namespace Aco228.Runners.UnitOfWork;

public interface IVoidWork<TReq> : IUnitOfWork
{
    Task Execute(TReq request);
}

public abstract class VoidWork<TReq> : UnitOfWork, IVoidWork<TReq>
{

    protected abstract Task InternalExecute(TReq request);
    
    public async Task Execute(TReq request)
    {
        await WrapVoidLogic(() => InternalExecute(request));
    }
    
}