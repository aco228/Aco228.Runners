using System.Collections.Concurrent;

namespace Aco228.Runners.Core.Tasks;

public class TaskStateMachine
{
    private int _limit = 5;
    private SemaphoreSlim? _semaphore;
    private readonly ConcurrentQueue<Func<Task>> _queue = new();
    private readonly List<Task> _tasks = new();
    private bool _running = false;
    private readonly object _lock = new();

    private SemaphoreSlim Semaphore => _semaphore ??= new SemaphoreSlim(_limit);

    public TaskStateMachine SetLimit(int limit)
    {
        if (_running)
            throw new InvalidOperationException("Cannot change limit while running.");

        _limit = limit;
        _semaphore = null; // reset so it gets recreated with new limit
        return this;
    }

    public void Run(Func<Task> func)
    {
        _queue.Enqueue(func);
        TryStart();
    }

    private void TryStart()
    {
        lock (_lock)
        {
            if (_running) return;
            _running = true;
            _ = RunLoop();
        }
    }

    private async Task RunLoop()
    {
        try
        {
            while (true)
            {
                if (_queue.TryDequeue(out var func))
                {
                    await Semaphore.WaitAsync();
                    var t = Task.Run(async () =>
                    {
                        try { await func(); }
                        finally { Semaphore.Release(); }
                    });
                    _tasks.Add(t);
                }
                else
                {
                    var running = Semaphore.CurrentCount < _limit;
                    if (!running)
                        break;

                    await Task.Delay(50);
                }
            }
        }
        finally
        {
            lock (_lock)
            {
                _running = false;
            }

            if (!_queue.IsEmpty)
                TryStart();
        }
    }

    public Task Wait() => Task.WhenAll(_tasks);
}