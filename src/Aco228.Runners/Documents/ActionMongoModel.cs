using System.Text.Json.Serialization;
using Aco228.Common;
using Aco228.MongoDb.Extensions;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aco228.Runners.Documents;

public interface IActionMongoModel
{
    object? GetInsideDocument();
}

public class ActionMongoModel<T> : IActionMongoModel
    where T : MongoDocument
{
    [JsonIgnore]
    public ObjectId? Id { get; set; }
    
    [BsonIgnore]
    public T? Document { get; set; }

    public ActionMongoModel() { }

    public ActionMongoModel(ObjectId id) : this()
    {
        Id = id;
    }
    
    public ActionMongoModel(string id) : this(ObjectId.Parse(id)) { }
    
    public ActionMongoModel(T document) : this(document.Id)
    {
        Id = document.Id;
        Document = document;
    }

    public T? GetDocument()
    {
        if (Id == null)
            return null;
        
        if (Document != null)
            return Document;
        
        Document = ServiceProviderHelper.GetService<IMongoRepo<T>>()!.NoTrack().FindById(Id.Value);
        return Document;
    }

    public async Task<T?> GetDocumentAsync()
    {
        if (Id == null)
            return default;
        
        if (Document != null)
            return Document;
        
        Document = await ServiceProviderHelper.GetService<IMongoRepo<T>>()!.NoTrack().FindByIdAsync(Id.Value);
        return Document;
    }

    public object? GetInsideDocument()
        => GetDocument();
}