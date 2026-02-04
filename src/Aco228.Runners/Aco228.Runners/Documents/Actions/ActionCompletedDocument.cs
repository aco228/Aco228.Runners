using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.Runners.Documents.Actions;

[BsonCollection("ActionCompleted")]
[BsonIgnoreExtraElements]
public class ActionCompletedDocument : ActionDocumentBase
{
    public string? ErrorMessage { get; set; }
    public required object? Response { get; set; }
}