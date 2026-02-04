using Aco228.Common;
using Aco228.MongoDb.Extensions.RepoExtensions;
using Aco228.MongoDb.Services;
using Aco228.Runners.Actions;
using Aco228.Runners.Actions.Documents;

namespace Aco228.Runners.Helpers;

public static class Actions
{
    public static IMongoRepo<ActionDocument> ActionDocumentRepo { get; set; }
    public static IMongoRepo<ActionDataDocument> ActionDataDocumentRepo { get; set; }
    
    public static T Get<T>()
        where T : IAction
    {
        var service = ServiceProviderHelper.Construct<T>();
        service.Name = nameof(T);
        service.RequestType = typeof(T).GetGenericArguments()[0];
        service.ResponseType = typeof(T).GetGenericArguments()[1];
        return service;
    }

    internal static async Task<ActionDocument> ScheduleInternal<T>(this T action, object request, string? reference = null)
        where T : IAction
    {
        var actionDocument = new ActionDocument
        {
            Name = action.Name,
            TypeDescription = action.GetType().FullName!,
            Status = ActionStatus.Scheduled,
            IsMain = true,
            Reference = reference,
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