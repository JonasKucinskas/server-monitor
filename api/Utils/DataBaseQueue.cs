using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class DatabaseQueue<T>
{
    private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
    private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
    private readonly Func<T, Task> _processItemAsync;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    public DatabaseQueue(Func<T, Task> processItemAsync)
    {
        _processItemAsync = processItemAsync ?? throw new ArgumentNullException(nameof(processItemAsync));
        Task.Run(ProcessQueueAsync); // Start background processing
    }

    public void Enqueue(T item)
    {
        _queue.Enqueue(item);
        _signal.Release(); // Signal that a new item is available
    }

    private async Task ProcessQueueAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            await _signal.WaitAsync(_cts.Token); // Wait for new items

            while (_queue.TryDequeue(out T item))
            {
                if (_cts.Token.IsCancellationRequested)
                    return; // Exit if cancellation was requested

                if (item == null)
                {
                    Console.WriteLine("Encountered a null item in the queue.");
                    continue; // Skip this iteration if the item is null
                }

                try
                {
                    await _processItemAsync(item); // Process the item (e.g., insert into database)
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
        _cts.Cancel(); // Graceful shutdown
        await Task.WhenAny(Task.Delay(-1), ProcessQueueAsync()); // Wait until the queue processing finishes or the task is cancelled
    }
}