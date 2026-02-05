namespace Aco228.Runners.UnitOfWork;

public interface IUnitOfWork
{
    string Name { get; }
}

public abstract class UnitOfWork : IUnitOfWork
{
    public virtual string Name => "";
    protected virtual ushort NumbersOfRetries { get; } = 1;
    protected virtual TimeSpan DelayBetweenRetries { get; } = TimeSpan.FromSeconds(1);
    protected bool ThrowIfException { get; } = true;


    internal async Task WrapVoidLogic(Func<Task> task)
    {
        for (int i = 0; i < NumbersOfRetries; i++)
        {
            try
            {
                await task();
            }
            catch (Exception e)
            {
                if (i == NumbersOfRetries - 1) // Last retry failed
                {
                    if (ThrowIfException)
                        throw;
                    
                    return;
                }
            
                await Task.Delay(DelayBetweenRetries);
            }
        }
    }
    
    internal async Task<T?> WrapValueLogic<T>(Func<Task<T>> task)
    {
        for (int i = 0; i < NumbersOfRetries; i++)
        {
            try
            {
                return await task();
            }
            catch (Exception e)
            {
                if (i == NumbersOfRetries - 1) // Last retry failed
                {
                    if (ThrowIfException)
                        throw;
                    
                    return default;
                }
            
                await Task.Delay(DelayBetweenRetries);
            }
        }

        return default;
    }

}