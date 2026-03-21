using Aco228.Common.Extensions;
using Aco228.Common.Models;
using Aco228.MongoDb.Helpers;
using Aco228.MongoDb.Services;
using Aco228.Runners.Core;
using Aco228.Runners.Core.Tasks;
using Aco228.Runners.Services;
using Aco228.Runners.Services.Background;
using Microsoft.Extensions.DependencyInjection;

namespace Aco228.Runners;

internal static class TaskCollection
{
    public static List<Type> Tasks { get; set; } = new();
}

public static class ServiceExtensions
{
    public static void RegisterRunnersRepositories<T>(this IServiceCollection services) where T : IMongoDbContext
    {
        services.RegisterRepositoriesFromAssembly<T>(typeof(ServiceExtensions).Assembly);
    }
    
    public static void RegisterTask<T>(this IServiceCollection services) where T : TaskBase
    {
        TaskCollection.Tasks.Add(typeof(T));
    }

    public static void ClearBackgroundTasks(this IServiceCollection services)
    {
        TaskCollection.Tasks.Clear();
    }

    public static void RegisterTaskManager(this IServiceCollection services)
    {
        services.RegisterBackgroundServices<TaskManagerService>();
    }

    public static void RegisterBackgroundServices<T>(this IServiceCollection services) where T : HostServiceBase
    {
        services.AddHostedService<T>();
    }

    public static void RegisterHostMachine(this IServiceCollection services)
    {
        var machineContract = HostMachineContract.CreateFromEnvironment();
        services.AddSingleton<IHostMachineService>(new HostMachineService(machineContract));
    }

    public static void ConfigureRunBackgroundServices(this IServiceCollection services, HostMachineContract machineContract)
    {
        services.AddHostedService<HeartbeatBackgroundService>();
        services.RegisterPostBuildActionAsync(async (pr) =>
        {
            await pr.GetService<IHostMachineService>()!.Initialize();
        });
    }
}