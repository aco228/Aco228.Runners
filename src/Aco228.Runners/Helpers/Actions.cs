using Aco228.Common;
using Aco228.Runners.Core;
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
        
        var service = ServiceProviderHelper.Construct<T>();
        var genericArguments = type.BaseType.GetGenericArguments();
        
        service.Name = typeof(T).Name;
        service.RequestType = genericArguments[0];
        service.ResponseType = genericArguments[1];
        
        return service;
    }
    
}