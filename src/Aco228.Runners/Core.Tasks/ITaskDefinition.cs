using Aco228.Common;
using Aco228.MongoDb.Extensions.MongoDocuments;
using Aco228.Runners.Documents;


namespace Aco228.Runners.Core.Tasks;

public interface ITaskDefinition
{
    string? Categories { get; }
    Type Type { get; }
    string Name { get; }
    DateTime? LastExecutionInUtc { get; }
    Task Update(TaskDocument? document = null);
}

public class TaskDefinition : ITaskDefinition
{
    public string? Categories { get; set; }
    public Type Type { get; set; }
    public string Name => Type.Name;
    internal TaskDocument Document { get; set; }
    public DateTime? LastExecutionInUtc => Document?.LastExecutionUtc;
    
    public TaskDefinition(Type type)
    {
        Type = type;
        var instance = Activator.CreateInstance(type) as TaskBase;
        Categories = instance!.Category;
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
        if(document != null) Document = document;
        
        Document.LastExecutionUtc = DateTime.UtcNow;
        await Document.InsertOrUpdateAsync();
    }
}