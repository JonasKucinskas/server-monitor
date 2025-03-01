using System;
using System.Threading;
using System.Threading.Tasks;

namespace monitor;

public class TimedTask
{
    private readonly Func<Task> _task;
    private readonly TimeSpan _interval;
    private CancellationTokenSource _cts;

    public TimedTask(Func<Task> task, TimeSpan interval)
    {
        _task = task ?? throw new ArgumentNullException(nameof(task));
        _interval = interval;
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _ = RunAsync(_cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _task(); // Execute the provided asynchronous task
                await Task.Delay(_interval, token); // Wait for the specified interval, respecting cancellation
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested.  Do nothing, loop will exit.
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately (e.g., logging) to prevent the timer from crashing.
                Console.WriteLine($"Error in periodic task: {ex.Message}");
                // Consider adding a delay here to avoid rapid-fire errors.
                await Task.Delay(_interval, token); // Wait before the next attempt.
            }
        }
    }
}