using Aco228.Runners.Core.Tasks.Types;

namespace Aco228.Runners.Consoler.Tasks;

public class TestTask : RunTask
{
    protected override async Task InternalExecute()
    {
        ConsoleLog("Ok");
    }
}