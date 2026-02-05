using Aco228.Common;
using Aco228.Runners.Core.Actions;

namespace Aco228.Runners.Helpers;

public static class Actions
{
    public static T Get<T>()
        where T : IAction
    {
        var type = typeof(T);
        if(type.BaseType == null)
            throw new InvalidOperationException("Action must inherit from IAction");

        var genericArguments = type.BaseType.GetGenericArguments();

        var service = ServiceProviderHelper.Construct<T>();
        
        service.Name = typeof(T).Name;
        service.RequestType = genericArguments[0];
        
        if (service.Type == ActionType.Result)
            service.ResponseType = genericArguments[1];
        
        return service;
    }
    
}