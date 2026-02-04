using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;

namespace Aco228.Runners.Documents.Actions;

public class ActionDocumentBase : MongoDocument
{
    [MongoIndex] public required string Name { get; set; }
    [MongoIndex] public string? Reference { get; set; }
    
    [MongoIndex] public ActionStatus Status { get; set; } = ActionStatus.Scheduled;
    [MongoIndex] public bool IsContextGroup { get; set; } = true;
    [MongoIndex] public string? ContextGroupId { get; set; } = null;
    public int ErrorCount { get; set; } = 0;
}