using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.Runners.Actions.Documents;

[BsonCollection("Actions")]
[BsonIgnoreExtraElements]
public class ActionDocument : MongoDocument
{
    [MongoIndex] public required string Name { get; set; }
    [MongoIndex] public required string TypeDescription { get; set; }
    [MongoIndex] public string? Reference { get; set; }
    
    [MongoIndex] public ActionStatus Status { get; set; } = ActionStatus.Scheduled;
    [MongoIndex] public bool IsMain { get; set; } = true;
    [MongoIndex] public DateTime? LastRun { get; set; } = null;

    public double ProcessingDurationInSeconds { get; set; } = 0;
    public double OverallDurationInSeconds { get; set; } = 0;

}