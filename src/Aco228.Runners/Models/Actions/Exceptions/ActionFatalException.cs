namespace Aco228.Runners.Models.Actions.Exceptions;

public class ActionFatalException : Exception
{
    public bool RaiseReporting { get; private set; } = true;
    public ActionFatalException(string message, bool raiseReporting = true) : base(message)
    {
        RaiseReporting = raiseReporting;
    }
}