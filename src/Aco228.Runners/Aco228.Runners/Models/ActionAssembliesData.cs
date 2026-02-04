using System.Reflection;

namespace Aco228.Runners.Models;

internal static class ActionAssembliesData
{
    public static HashSet<Assembly> Assemblies { get; set; } = new();

    public static bool TryGetType(string typeDefinition, out Type type)
    {
        type = Type.GetType(typeDefinition);
        if(type != null)
            return true;
        
        foreach (var assembly in Assemblies)
        {
            var returnType = assembly.GetType(typeDefinition, throwOnError: false);
            if (returnType != null)
            {
                type = returnType;
                return true;
            }
        }

        return false;
    }
}