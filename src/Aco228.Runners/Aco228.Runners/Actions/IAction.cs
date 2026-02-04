namespace Aco228.Runners.Actions;

public interface IAction
{
    string Name { get; internal set; }
    internal Type RequestType { get; set; }
    internal Type ResponseType { get; set; }
}
