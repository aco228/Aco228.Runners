using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.Runners.Documents.Actions;

[BsonCollection("ActionRunning")]
[BsonIgnoreExtraElements]
public class ActionRunDocument : ActionDocumentBase
{
    [MongoIndex] public DateTime? LastRun { get; set; } = null;
    [MongoIndex] public required string TypeDescription { get; set; }
    [MongoIndex] public long LastInteractionUtcTc { get; set; }
    
    public string? LockBy { get; set; }
    public long? LockTimeTs { get; set; }
    public long? ExecutionStartedTs { get; set; }
}