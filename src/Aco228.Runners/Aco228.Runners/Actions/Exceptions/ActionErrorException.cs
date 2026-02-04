namespace Aco228.Runners.Actions.Exceptions;

public class ActionErrorException : Exception
{
    public string ErrorMessage { get; set; }
    
    public ActionErrorException(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}