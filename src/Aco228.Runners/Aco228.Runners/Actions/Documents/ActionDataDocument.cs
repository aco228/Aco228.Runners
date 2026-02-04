using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.Runners.Actions.Documents;

[BsonCollection("ActionData")]
[BsonIgnoreExtraElements]
public class ActionDataDocument : MongoDocument
{
    [MongoIndex] public ObjectId ActionId { get; set; }
    public required object Request { get; set; }
    public required string RequestType { get; set; }
    public required string ResponseType { get; set; }
    
    public object? Response { get; set; }
}