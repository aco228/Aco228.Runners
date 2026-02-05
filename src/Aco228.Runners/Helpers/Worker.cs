using Aco228.Runners.UnitOfWork;

namespace Aco228.Runners.Helpers;

public static class Worker
{

    public static TVoidWorker Get<TVoidWorker>()
    {
        var type = typeof(TVoidWorker);
        if (!typeof(IUnitOfWork).IsAssignableFrom(type))
            throw new InvalidOperationException("");

        var generics = type.GenericTypeArguments;
        
        return (TVoidWorker)Activator.CreateInstance(type, generics);
    }
}