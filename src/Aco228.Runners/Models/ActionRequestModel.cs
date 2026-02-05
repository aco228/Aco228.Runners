namespace Aco228.Runners.Models;

public class ActionRequestModel
{
    public string RequestType { get; set; }
    public Type? RequestTypeAsType => Type.GetType(RequestType);
    public object? Request { get; set; }
    
    public T? GetRequest<T>() => (T?)Request;
}