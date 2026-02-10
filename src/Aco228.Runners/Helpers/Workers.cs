using Aco228.Common;
using Aco228.Runners.Core.Worker;

namespace Aco228.Runners.Helpers;

public static class Workers
{
    public static T Get<T>()
        where T : IUnitOfWork
    {
        var worker = ServiceProviderHelper.Construct<T>();
        return worker;
    }
    
}