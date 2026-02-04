// See https://aka.ms/new-console-template for more information

using Aco228.Common.Extensions;
using Aco228.Runners;
using Aco228.Runners.Consoler.Actions;
using Aco228.Runners.Consoler.Db;
using Aco228.Runners.Consoler.Runners;
using Aco228.Runners.Helpers;
using Microsoft.Extensions.DependencyInjection;

var builder = new ServiceCollection();
builder.ConfigureActionServices<ILocalDbContext>();
var provider = await builder.BuildCollection();

Console.WriteLine("Hello, World!");

// var service = await Actions.Get<DummyAction>().Execute(1);
var actionDocument = await Actions.Get<DummyAction>().Schedule(1, "aco");

Console.WriteLine("Result = ");
