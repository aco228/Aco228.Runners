// See https://aka.ms/new-console-template for more information

using Aco228.Common.Extensions;
using Aco228.Runners;
using Aco228.Runners.Consoler.Db;
using Aco228.Runners.Consoler.Tasks;
using Aco228.Runners.Consoler.Workers;
using Aco228.Runners.Helpers;
using Aco228.WService;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;

Env.Load();

var builder = new ServiceCollection();
builder.RegisterApiServices(typeof(Program).Assembly);
builder.ConfigureRunActionServices<ILocalDbContext>();
builder.RegisterServicesFromAssembly(typeof(Program).Assembly);
builder.ConfigureRunActionBackgroundServices(new()
{
    MachineName = "VoicemodLenovo",
    ApplicationName = typeof(Program).Assembly.GetName().Name ?? "unknown"
});
var provider = await builder.BuildCollection();


await RunTasks.Execute<TestTask>();
// await Actions.Get<CollectGithubAction>().GetPromiseFor(15);
int a = 0;


//
// var postId = new Random().Next(1, 100);
// // await Actions.Get<DummyGithubAction>().Schedule(postId, Guid.NewGuid().ToString());
// await HostedServiceRunner.RunAsync(provider);;