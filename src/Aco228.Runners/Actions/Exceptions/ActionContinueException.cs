namespace Aco228.Runners.Actions.Exceptions;

public class ActionContinueException : Exception
{
    public string? Message { get; set; }
    
    public ActionContinueException() { }

    public ActionContinueException(string message)
    {
        Message = message;
    }
}