using System.Collections.Concurrent;

public class DatabaseQueue<T>
{
    private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
    private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
    private readonly Func<T, Task> _processItemAsync;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public DatabaseQueue(Func<T, Task> processItemAsync)
    {
        _processItemAsync = processItemAsync ?? throw new ArgumentNullException(nameof(processItemAsync));
        Task.Run(ProcessQueueAsync); 
    }

    public void Enqueue(T item)
    {
        _queue.Enqueue(item);
        _signal.Release(); 
    }

    private async Task ProcessQueueAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            await _signal.WaitAsync(_cts.Token);

            while (_queue.TryDequeue(out T item))
            {
                if (_cts.Token.IsCancellationRequested)
                    return;

                if (item == null)
                {
                    Console.WriteLine("Encountered a null item in the queue.");
                    continue;
                }

                try
                {
                    await _processItemAsync(item); 
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing item: {ex.Message}");
                }
            }
        }
    }

    public async Task StopAsync()
    {
        _cts.Cancel(); 
        await Task.WhenAny(Task.Delay(-1), ProcessQueueAsync()); 
    }
}