using Aco228.Runners.UnitOfWork;

namespace Aco228.Runners.Consoler.Runners;

public class DummyReturnWork : ValueWork<string, int>
{
    
    protected override async Task<int> Execute(string request)
    {
        return 69;
    }
}