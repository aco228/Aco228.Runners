using Aco228.Common;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Services;
using Aco228.Runners.Documents.Actions;

namespace Aco228.Runners.Extensions;

public static class ActionCompletedDocumentExtensions
{
    public static async Task<ActionCompletedDocument> SetErrorWithMessage(this ActionRunDocument document, string message)
    {
        var completedDocument = new ActionCompletedDocument()
        {
            Id = document.Id,
            Name = document.Name,
            Status = ActionStatus.Failed,
            Reference = document.Reference,
            IsContextGroup = document.IsContextGroup,
            ContextGroupId = document.ContextGroupId,
            ErrorMessage = message,
        };

        await ServiceProviderHelper.GetService<IMongoRepo<ActionRunDocument>>()!.DeleteAsync(document);
        await ServiceProviderHelper.GetService<IMongoRepo<ActionCompletedDocument>>()!.InsertOrUpdateAsync(completedDocument);
        
        return completedDocument;
    }
}