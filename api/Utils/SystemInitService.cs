public class SystemInitService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SystemInitService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<Database>();
        var systems = await database.FetchAllSystems();

        foreach (var system in systems)
        {
            _ = Task.Run(() => 
                SshConnection.Instance.StartSendingRequests(
                    system.ip,
                    system.port,
                    "monitor",
                    5),
                stoppingToken);
        }
    }
}
