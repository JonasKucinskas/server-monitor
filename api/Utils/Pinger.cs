using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

public static class Pinger
{
    public static async Task PingAsync(NetworkService target, HttpClient client, Database db)
    {
        string url = $"http://{target.ip}:{target.port}";
        client.Timeout = TimeSpan.FromSeconds(target.timeout);

        while (true)
        {
            Console.WriteLine($"{DateTime.Now} pinging {target.name} every {target.interval}.");
            PingData ping = new PingData();
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                HttpResponseMessage response = await client.GetAsync(url);
                stopwatch.Stop();

                ping.responseTime = stopwatch.ElapsedMilliseconds;
                ping.serviceId = target.id;
                ping.errorMessage = "";
                ping.isUp = (int)response.StatusCode == target.expected_status;

                target.last_checked = DateTime.Now;

                Console.WriteLine($"[{target.last_checked}] {target.name} ({target.ip}:{target.port}) - status: {(int)response.StatusCode}");
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"[{DateTime.Now}] {target.name} ({target.ip}:{target.port}) - timeout after {target.timeout}s");
                
                ping.responseTime = 0;//indicate timeout
                ping.errorMessage = $"Timeout after {target.timeout} seconds.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] {target.name} ({target.ip}:{target.port}) - error: {ex.Message}");
                ping.errorMessage = $"Error: {ex.Message}";
            }

            await db.InsertNetworkServicePing(ping);
            await Task.Delay(target.interval * 1000);//wait for next ping
        }
    }
}