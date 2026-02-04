using Aco228.Common;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Models;
using Aco228.MongoDb.Services;
using Aco228.Runners.Actions;
using Aco228.Runners.Documents.Actions;

namespace Aco228.Runners.Helpers;

public static class Actions
{
    public static IMongoRepo<ActionRunDocument> ActionDocumentRepo { get; set; }
    public static IMongoRepo<ActionCompletedDocument> ActionCompletedRepo { get; set; }
    public static IMongoRepo<ActionDataDocument> ActionDataDocumentRepo { get; set; }
    
    public static T Get<T>()
        where T : IAction
    {
        var type = typeof(T);
        if(type.BaseType == null)
            throw new InvalidOperationException("Action must inherit from IAction");
        
        var service = ServiceProviderHelper.Construct<T>();
        var genericArguments = type.BaseType.GetGenericArguments();
        
        service.Name = typeof(T).Name;
        service.RequestType = genericArguments[0];
        service.ResponseType = genericArguments[1];
        
        return service;
    }

    internal static async Task<ActionRunDocument> ScheduleInternal<T>(this T action, object request, string? reference = null)
        where T : IAction
    {
        var actionDocument = new ActionRunDocument
        {
            Name = action.Name,
            TypeDescription = action.GetType().FullName!,
            Status = ActionStatus.Scheduled,
            IsContextGroup = true,
            Reference = reference,
            LastInteractionUtcTc = DT.GetUnix(),
        };
        await ActionDocumentRepo.InsertOrUpdateAsync(actionDocument);

        var actionDataDocument = new ActionDataDocument()
        {
            ActionId = actionDocument.Id,
            Request = request,
            RequestType = action.RequestType.FullName!,
            ResponseType = action.ResponseType.FullName!,
        };
        await ActionDataDocumentRepo.InsertOrUpdateAsync(actionDataDocument);
        return actionDocument;
    }
}