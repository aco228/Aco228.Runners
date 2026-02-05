using System.Text.Json;
using Aco228.Runners.Documents;

namespace Aco228.Runners.Helpers;

public static class ActionObjectModelHelper
{
    public static ActionObjectModel? Get(object? input)
    {
        if(input == null) return null;
        
        var model = new ActionObjectModel()
        {
            Type = input.GetType().FullName,
            Value = JsonSerializer.Serialize(input)
        };
        return model;
    }

    public static T? Get<T>(this ActionObjectModel? input)
    {
        if(input == null) return default;
        return JsonSerializer.Deserialize<T>(input.Value);
    }
}