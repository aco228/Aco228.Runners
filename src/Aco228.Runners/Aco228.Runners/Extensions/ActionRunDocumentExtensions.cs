using Aco228.Common.Extensions;
using Aco228.MongoDb.Models;
using Aco228.Runners.Documents.Actions;

namespace Aco228.Runners.Extensions;

public static class ActionRunDocumentExtensions
{
    public static bool TryReleaseLock(this ActionRunDocument actionDocument, int maximumMinutes)
    {
        if(string.IsNullOrEmpty(actionDocument.LockBy) || actionDocument.LockTimeTs == null)
            return false;

        if ((actionDocument.LockTimeTs != null && actionDocument.LockTimeTs.GetMinutesDifferenceUTC() > maximumMinutes + 10)
            || (actionDocument.ExecutionStartedTs != null && actionDocument.ExecutionStartedTs.GetMinutesDifferenceUTC() > maximumMinutes + 20))
        {
            actionDocument.ReleaseLock();
            return true;
        }

        return false;
    }

    public static bool CanBeScheduled(this ActionRunDocument actionDocument, string lockKey)
    {
        if(!string.IsNullOrEmpty(actionDocument.LockBy))
            return false;
        
        actionDocument.ReleaseLock();
        actionDocument.LockBy = lockKey;
        actionDocument.LockTimeTs = DT.GetUnix();
        actionDocument.Status = ActionStatus.ScheduleLock;
        return true;
    }

    public static ActionRunDocument ReleaseLock(this ActionRunDocument actionDocument)
    {
        actionDocument.ExecutionStartedTs = null;
        actionDocument.LockBy = null;
        actionDocument.LockTimeTs = null;
        actionDocument.Status = ActionStatus.Scheduled;
        return actionDocument;
    }
}