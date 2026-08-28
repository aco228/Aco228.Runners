using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.Runners.Documents;

[BsonCollection("HostMachines")]
[BsonIgnoreExtraElements]
public class HostMachineDocument : MongoDocument
{
    public required string MachineName { get; set; }
    public required string ApplicationName { get; set; }
    
    public DateTime? LastTickUtc { get; set; }
    public DateTime? StartTimeUtc { get; set; }
}