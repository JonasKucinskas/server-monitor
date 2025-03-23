using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

public static class Pinger
{
    public static async Task PingOnceAsync(NetworkService target, HttpClient client, Database db, CancellationToken cancellationToken)
    {
        string url = $"http://{target.ip}:{target.port}";

        try
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
            stopwatch.Stop();

            PingData ping = new PingData
            {
                responseTime = stopwatch.ElapsedMilliseconds,
                serviceId = target.id,
                errorMessage = "",
                isUp = (int)response.StatusCode == target.expected_status
            };

            target.last_checked = DateTime.Now;

            Console.WriteLine($"{target.name} ({target.ip}:{target.port}) - status: {(int)response.StatusCode}");
            
            await db.InsertNetworkServicePing(ping);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"[{DateTime.Now}] {target.name} ({target.ip}:{target.port}) - timeout after {target.timeout}s");

            PingData ping = new PingData
            {
                responseTime = 0,
                errorMessage = $"Timeout after {target.timeout} seconds."
            };
            await db.InsertNetworkServicePing(ping);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now}] {target.name} ({target.ip}:{target.port}) - error: {ex.Message}");
            
            PingData ping = new PingData
            {
                responseTime = 0,
                errorMessage = $"Error: {ex.Message}"
            };
            await db.InsertNetworkServicePing(ping);
        }
    }
}