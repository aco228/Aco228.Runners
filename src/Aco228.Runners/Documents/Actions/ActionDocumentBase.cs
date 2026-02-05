using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson;

namespace Aco228.Runners.Documents.Actions;

public class ActionDocumentBase : MongoDocument
{
    [MongoIndex] public required string Name { get; set; }
    [MongoIndex] public string? Reference { get; set; }
    [MongoIndex] public ObjectId? ParentId { get; set; }
    
    [MongoIndex] public ActionStatus Status { get; set; } = ActionStatus.Waiting;
    [MongoIndex] public bool IsContextGroup { get; set; } = true;
    [MongoIndex] public string? ContextGroupId { get; set; } = null;
    public int ErrorCount { get; set; } = 0;
}