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
                await _task(); 
                await Task.Delay(_interval, token); 
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in periodic task: {ex.Message}");
                await Task.Delay(_interval, token); 
            }
        }
    }
}