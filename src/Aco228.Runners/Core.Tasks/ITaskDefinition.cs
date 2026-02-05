using Aco228.Common;
using Aco228.MongoDb.Extensions.MongoDocuments;
using Aco228.Runners.Core.Tasks.Documents;

namespace Aco228.Runners.Core.Tasks;

public interface ITaskDefinition
{
    string? Categories { get; }
    Type Type { get; }
    string Name { get; }
    DateTime? LastExecutionInUtc { get; }
    Task Update();
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

    public async Task Update()
    {
        Document.LastExecutionUtc = DateTime.UtcNow;
        await Document.InsertOrUpdateAsync();
    }
}