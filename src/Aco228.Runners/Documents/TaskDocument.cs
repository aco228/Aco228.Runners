using Aco228.MongoDb.Models;
using Aco228.MongoDb.Models.Attributes;
using Aco228.Runners.Models.Timings;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.Runners.Documents;

[BsonCollection("Tasks")]
[BsonIgnoreExtraElements]
public class TaskDocument : MongoDocument
{
    [MongoIndex] public required string Name { get; set; }
    public HourWindow From { get; set; }
    public HourWindow To { get; set; }
    public DelayWindow Delay { get; set; }
    public DelayWindow? DelaySuccess { get; set; }
    public List<DayOfWeek>? OnlyOnDays { get; set; }
    public DateTime? LastExecutionUtc { get; set; }
    public DateTime? LastSuccessExecutionInUtc { get; set; }
}