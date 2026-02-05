using Aco228.Runners.UnitOfWork;

namespace Aco228.Runners.Consoler.Runners;

public class DummyVoidWork : VoidWork<int?>
{
    public override string Name => "DummyVoidWork";
    protected override ushort NumbersOfRetries => 3;
    protected override TimeSpan DelayBetweenRetries => TimeSpan.FromSeconds(1);

    protected override async Task InternalExecute(int? request)
    {
        Console.WriteLine("OK OK");
    }
}