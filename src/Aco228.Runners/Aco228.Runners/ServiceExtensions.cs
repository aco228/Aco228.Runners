using Aco228.Common.Extensions;
using Aco228.MongoDb.Helpers;
using Aco228.MongoDb.Services;
using Aco228.Runners.Actions.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace Aco228.Runners;

public static class ServiceExtensions
{
    public static void ConfigureActionServices<TDbContext>(this IServiceCollection services)
        where TDbContext : IMongoDbContext
    {   
        services.RegisterServicesFromAssembly(typeof(ServiceExtensions).Assembly);
        services.RegisterRepositoriesFromAssembly<TDbContext>();
        services.RegisterPostBuildAction((pr) =>
        {
            Helpers.Actions.ActionDocumentRepo = pr.GetService<IMongoRepo<ActionDocument>>()!;
            Helpers.Actions.ActionDataDocumentRepo = pr.GetService<IMongoRepo<ActionDataDocument>>()!;
        });
    }
}