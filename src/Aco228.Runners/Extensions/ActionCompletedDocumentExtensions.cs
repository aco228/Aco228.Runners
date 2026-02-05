using Aco228.Common;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Services;
using Aco228.Runners.Documents.Actions;
using Aco228.Runners.Helpers;
using MongoDB.Bson;

namespace Aco228.Runners.Extensions;

public static class ActionCompletedDocumentExtensions
{
    public static async Task<ActionCompletedDocument> MoveToFailed(this ActionRunDocument document, string message)
        => await document.MoveToCompleted( message, ActionStatus.Failed, null);
    
    public static async Task<ActionCompletedDocument> MoveToFinished(this ActionRunDocument document, object result)
        => await document.MoveToCompleted(string.Empty, ActionStatus.Finished, result);
    
    public static async Task<ActionCompletedDocument> MoveToCompleted(
        this ActionRunDocument document, 
        string message,
        ActionStatus actionStatus,
        object? response)
    {
        if(!(actionStatus is ActionStatus.Failed or ActionStatus.Finished))
            throw new ArgumentException("Action must be failed or finished");
        
        document.Status = actionStatus;
        var completedDocument = new ActionCompletedDocument()
        {
            Id = document.Id,
            Name = document.Name,
            Status = actionStatus,
            Reference = document.Reference,
            IsContextGroup = document.IsContextGroup,
            ContextGroupId = document.ContextGroupId,
            ErrorMessage = message,
            Response = ActionObjectModelHelper.Get(response),
        };

        await ServiceProviderHelper.GetService<IMongoRepo<ActionRunDocument>>()!.DeleteAsync(document);
        await ServiceProviderHelper.GetService<IMongoRepo<ActionCompletedDocument>>()!.InsertOrUpdateAsync(completedDocument);
        
        return completedDocument;
    }
}