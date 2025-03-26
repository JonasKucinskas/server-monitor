using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

public static class Pinger
{
    public static async Task PingOnceAsync(NetworkService target, Database db, CancellationToken cancellationToken)
    {
        string url = $"http://{target.ip}:{target.port}";

        HttpClient client = new HttpClient();
        client.Timeout = new TimeSpan(0, 0, 0, 0, target.timeout);

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

            if ((int)response.StatusCode != target.expected_status)
            {
                ping.responseTime = target.timeout;
                ping.errorMessage = $"Returned {response.StatusCode} code";
            }

            Console.WriteLine($"{DateTime.Now}: {target.name} ({target.ip}:{target.port}) - {(int)response.StatusCode}");
            
            await db.InsertNetworkServicePing(ping);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"{DateTime.Now}: {target.name} ({target.ip}:{target.port}) - timeout after {target.timeout} ms");

            PingData ping = new PingData
            {
                serviceId = target.id,
                isUp = false,
                responseTime = target.timeout,
                errorMessage = $"Timeout after {target.timeout} ms."
            };
            await db.InsertNetworkServicePing(ping);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now}: {target.name} ({target.ip}:{target.port}) - error: {ex.Message}");
        
            PingData ping = new PingData
            {
                serviceId = target.id,
                isUp = false,
                responseTime = target.timeout,
                errorMessage = $"Error: {ex.Message}"
            };
            await db.InsertNetworkServicePing(ping);
        }
    }
}