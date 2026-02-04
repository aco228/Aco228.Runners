using Aco228.Runners.Actions;
using Aco228.Runners.Actions.Exceptions;

namespace Aco228.Runners.Consoler.Actions;

public class DummyAction : ActionBase<int, string>
{
    protected override ushort NumbersOfRetries => 3;
    private int _num = 0;
    
    protected override async Task<string> ExecuteInternal(int request)
    {
        Console.WriteLine($"DummyAction {_num} {request}");
        
        if (_num >= 3)
            return "okk";
        
        _num++;
        throw new ActionContinueException();
    }
}