using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

public class PingerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpClient _httpClient = new();

    public PingerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Database>();

            string agentIp = "localhost";
            List<NetworkService> targets = await db.FetchAllNetworkServices(agentIp);
            List<Task> tasks = targets.Select(target => Pinger.PingAsync(target, _httpClient, db)).ToList();

            await Task.WhenAll(tasks);
        }
    }
}
