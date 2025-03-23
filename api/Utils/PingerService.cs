using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

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

            List<NetworkService> targets = await db.FetchAllNetworkServices(null);
            Console.WriteLine(targets.Count);


            List<Task> tasks = targets.Select(target => Pinger.PingOnceAsync(target, _httpClient, db, stoppingToken)).ToList();

            await Task.WhenAll(tasks);

            await Task.Delay(1000);
        }
    }
}
