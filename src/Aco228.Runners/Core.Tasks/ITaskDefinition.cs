using Aco228.Common;
using Aco228.MongoDb.Extensions.MongoDocuments;
using Aco228.Runners.Documents;
using Aco228.Runners.Models.Timings;


namespace Aco228.Runners.Core.Tasks;

public interface ITaskDefinition
{
    string? Category { get; }
    Type Type { get; }
    string Name { get; }
    TaskDocument Document { get;}
    DateTime? LastExecutionInUtc { get; }
    DateTime? LastSuccessExecutionInUtc { get; }
    Task Update(TaskDocument? document = null);
}

public class TaskDefinition : TaskDateDefinition, ITaskDefinition
{
    public string? Category { get; set; }
    public Type Type { get; set; }
    public string Name => Type.Name;
    public TaskDocument Document { get; internal set; }
    public override HourWindow From => Document.From;
    public override HourWindow To => Document.To;
    public override DelayWindow Delay => Document.Delay;
    public override DelayWindow? DelaySuccess => Document.DelaySuccess;
    public override List<DayOfWeek>? OnlyOnDays => Document.OnlyOnDays;
    public override DateTime? LastExecutionInUtc => Document?.LastExecutionUtc;
    public override DateTime? LastSuccessExecutionInUtc => Document?.LastSuccessExecutionInUtc;

    public TaskDefinition(Type type)
    {
        Type = type;
    }

    public TaskBase CreateInstance()
    {
        var instance = Activator.CreateInstance(Type) as TaskBase;
        Category = instance!.Category;
        return instance;
    }

    public TaskBase Initialize()
    {
        var task =  ServiceProviderHelper.ConstructByType(Type) as TaskBase;
        task.Document = Document;
        task.Name = Name;
        task.TaskDefinition = this;
        return task;
    }

    public async Task Update(TaskDocument? document = null)
    {
        if (document != null)
        {
            Document = document;
        }
        
        Document.LastExecutionUtc = DateTime.UtcNow;
        await Document.InsertOrUpdateAsync();
    }
}