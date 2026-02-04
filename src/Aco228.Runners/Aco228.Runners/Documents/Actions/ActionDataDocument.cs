using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.Runners.Documents.Actions;

[BsonCollection("ActionData")]
[BsonIgnoreExtraElements]
public class ActionDataDocument : MongoDocument
{
    public required ObjectId ActionId { get; set; }

    public List<ActionDataDocumentLog> Logs { get; set; } = new();
}

public class ActionDataDocumentLog
{
    public string Message { get; set; } = "";
    public DateTime DateUtc { get; set; }

    public ActionDataDocumentLog(string message)
    {
        Message = message;
        DateUtc = DateTime.UtcNow;
    }
}