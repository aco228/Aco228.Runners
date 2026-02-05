using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.Runners.Core.Tasks.Documents;

[BsonCollection("Tasks")]
[BsonIgnoreExtraElements]
public class TaskDocument : MongoDocument
{
    public string Name { get; set; }
    public string ServerName { get; set; }
    public DateTime? LastExecutionUtc { get; set; }
}