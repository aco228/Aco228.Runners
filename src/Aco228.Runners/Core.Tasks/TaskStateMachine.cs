using System.Collections.Concurrent;

namespace Aco228.Runners.Core.Tasks;

public class TaskStateMachine
{
    private int _limit = 5;
    private SemaphoreSlim? _semaphore;
    private readonly ConcurrentQueue<(Func<Task> func, object? entry)> _queue = new();
    private readonly List<Task> _tasks = new();
    private bool _running = false;
    private readonly object _lock = new();
    private Task? _loopTask;

    public Action<Exception, object?>? OnError { get; set; }

    private SemaphoreSlim Semaphore => _semaphore ??= new SemaphoreSlim(_limit);

    public TaskStateMachine SetLimit(int limit)
    {
        if (_running)
            throw new InvalidOperationException("Cannot change limit while running.");
        _limit = limit;
        _semaphore = null;
        return this;
    }

    public void Schedule(Func<Task> func)
        => Enqueue(func, null);

    public void ScheduleWith<T>(Func<Task> func, T entry)
        => Enqueue(func, entry);

    private void Enqueue(Func<Task> func, object? entry)
    {
        _queue.Enqueue((func, entry));
        TryStart();
    }
    
    private void TryStart()
    {
        lock (_lock)
        {
            if (_running) return;
            _running = true;
            _loopTask = RunLoop();
        }
    }

    private async Task RunLoop()
    {
        try
        {
            while (true)
            {
                if (_queue.TryDequeue(out var item))
                {
                    await Semaphore.WaitAsync();
                    var t = Task.Run(async () =>
                    {
                        try { await item.func(); }
                        catch (Exception ex)
                        {
                            OnError?.Invoke(ex, item.entry);
                            if (item.entry != null)
                                _queue.Enqueue(item);
                        }
                        finally { Semaphore.Release(); }
                    });
                    _tasks.Add(t);
                }
                else
                {
                    var running = Semaphore.CurrentCount < _limit;
                    if (!running) break;
                    await Task.Delay(50);
                }
            }
        }
        finally
        {
            lock (_lock) { _running = false; }
            if (!_queue.IsEmpty) TryStart();
        }
    }
    public async Task Wait()
    {
        if (_loopTask != null)
            await _loopTask;

        // Drain all semaphore slots — proves every Task.Run has hit Semaphore.Release()
        for (int i = 0; i < _limit; i++)
            await Semaphore.WaitAsync();

        // Reset so the machine can be reused
        Semaphore.Release(_limit);
    }
}