// See https://aka.ms/new-console-template for more information

using Aco228.Common.Extensions;
using Aco228.Runners;
using Aco228.Runners.Consoler.Actions;
using Aco228.Runners.Consoler.Db;
using Aco228.Runners.Helpers;
using Aco228.Runners.HostedServices;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;

Env.Load();

var builder = new ServiceCollection();
builder.ConfigureRunActionServices<ILocalDbContext>();
builder.RegisterServicesFromAssembly(typeof(Program).Assembly);
builder.ConfigureRunActionBackgroundServices(new()
{
    MachineName = "VoicemodLenovo",
    ApplicationName = typeof(Program).Assembly.GetName().Name ?? "unknown"
});
var provider = await builder.BuildCollection();

var actionDocument = await Actions.Get<DummyAction>().Schedule(1, Guid.NewGuid().ToString());
await HostedServiceRunner.RunAsync(provider);