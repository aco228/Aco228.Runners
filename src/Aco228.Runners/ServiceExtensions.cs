using System.Reflection;
using Aco228.Common.Extensions;
using Aco228.MongoDb.Helpers;
using Aco228.MongoDb.Services;
using Aco228.Runners.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Aco228.Runners;

public static class ServiceExtensions
{
    public static void ConfigureRunActionAssembly(this IServiceCollection services, Assembly assembly)
    {
        // ActionAssembliesData.Assemblies.Add(assembly);
    }
    
    public static void ConfigureRunActionServices<TDbContext>(this IServiceCollection services)
        where TDbContext : IMongoDbContext
    {
        // services.ConfigureRunActionAssembly( typeof(TDbContext).Assembly);
        // services.RegisterServicesFromAssembly(typeof(ServiceExtensions).Assembly);
        // services.RegisterRepositoriesFromAssembly<TDbContext>(typeof(ServiceExtensions).Assembly);
        // services.RegisterPostBuildAction((pr) =>
        // {
        //     Helpers.Actions.ActionDocumentRepo = pr.GetService<IMongoRepo<ActionRunDocument>>()!;
        //     Helpers.Actions.ActionCompletedRepo = pr.GetService<IMongoRepo<ActionCompletedDocument>>()!;
        // });
    }

    public static void ConfigureRunActionBackgroundServices(this IServiceCollection services, HostMachineContract machineContract)
    {
        // services.AddSingleton<IHostMachineService>(new HostMachineService(machineContract));
        // services.AddHostedService<ActionBackgroundService>();
        // services.AddHostedService<HeartbeatBackgroundService>();
        // services.RegisterPostBuildActionAsync(async (pr) =>
        // {
        //     await pr.GetService<IHostMachineService>()!.Initialize();
        // });
    }
}