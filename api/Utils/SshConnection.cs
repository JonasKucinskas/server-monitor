using Renci.SshNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public sealed class SshConnection
{
    private static readonly Lazy<SshConnection> _instance = new Lazy<SshConnection>(() => new SshConnection(Database.Instance));
    public static SshConnection Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, SshClient> _sshClients = new ConcurrentDictionary<string, SshClient>();
    private readonly ConcurrentDictionary<string, Timer> _timers = new ConcurrentDictionary<string, Timer>();
    private readonly ConcurrentDictionary<string, int> _currentIntervals = new ConcurrentDictionary<string, int>();
    private readonly Database _dbService;

    private SshConnection(Database dbService)
    {
        _dbService = dbService;
    }

    public async Task Connect(string agentIpAddress, int agentPort, string username)
    {
        string serverKey = $"{agentIpAddress}:{agentPort}:{username}";
        while (true)
        {
            try
            {
                var privateKeyFile = new PrivateKeyFile("privateKey.pem");
                var authenticationMethod = new PrivateKeyAuthenticationMethod(username, privateKeyFile);
                var connectionInfo = new Renci.SshNet.ConnectionInfo(agentIpAddress, agentPort, username, authenticationMethod);

                var sshClient = new SshClient(connectionInfo);
                sshClient.Connect();

                _sshClients.AddOrUpdate(serverKey, sshClient, (key, oldValue) => sshClient);

                Console.WriteLine($"Connected to SSH server: {agentIpAddress}:{agentPort}:{username}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now}: Error connecting to {agentIpAddress}:{agentPort}:{username}: {ex.Message}. Retrying in 5 seconds...");
            }

            await Task.Delay(5000);
        }
    }

    public string RunCmd(string agentIpAddress, int port, string command)
    {
        string serverKey = $"{agentIpAddress}:{port}:{"monitor"}";
        if (_sshClients.TryGetValue(serverKey, out SshClient sshClient))
        {
            try
            {
                using (SshCommand cmd = sshClient.RunCommand(command))
                {
                    if (cmd.Result == "ping")
                    {
                        return null;
                    }
                    return cmd.Result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now}: Error running {command} command on {agentIpAddress}:{port}:{"monitor"}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"{DateTime.Now}: SSH client for {serverKey} not found.");
        }

        return null;
    }

    private async void RequestData(string agentIpAddress, int agentPort, string username)
    {
        string serverKey = $"{agentIpAddress}:{agentPort}:{username}";
        if (_sshClients.TryGetValue(serverKey, out SshClient sshClient))
        {
            try
            {
                using (SshCommand cmd = sshClient.RunCommand("echo 'dataRequest'"))
                {
                    Console.WriteLine(serverKey);
                    DataPackage data = Deserealizer.Deserialize<DataPackage>(cmd.Result);

                    var system = await _dbService.FetchSystemByIpAsync(agentIpAddress);
                    var user = await _dbService.FetchSystemOwnerAsync(system);
                    var rules = await _dbService.FetchNotificationRulesAsync(user.id, agentIpAddress);

                    List<Notification> notifications = NotificationManager.GetNotificationData(rules, data);

                    if (notifications.Count > 0)
                    {
                        foreach (var notification in notifications)
                        {
                            Notification addedNotification = await _dbService.InsertNotificationAsync(notification);
                            List<Process> processes = NotificationManager.GetProcessList(addedNotification, agentIpAddress, agentPort);

                            foreach (var process in processes)
                            {
                                await _dbService.InsertProcess(process);
                            }
                        }
                    }

                    await _dbService.InsertServerMetricsAsync(data, agentIpAddress);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now}: Error requesting data from {agentIpAddress}:{agentPort}:{username}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"{DateTime.Now}: SSH client for {agentIpAddress}:{agentPort}:{username} not found.");
        }
    }

    public async Task StartSendingRequests(string agentIpAddress, int agentPort, string username, int intervalInSeconds)
    {
        string serverKey = $"{agentIpAddress}:{agentPort}:{username}";
        _currentIntervals.AddOrUpdate(serverKey, intervalInSeconds, (key, oldValue) => intervalInSeconds);

        bool _isReconnecting = false;
        var timer = new Timer(async _ =>
        {
            if (_isReconnecting) return;
            if (!_sshClients.TryGetValue(serverKey, out SshClient sshClient) || sshClient == null || !sshClient.IsConnected)
            {
                _isReconnecting = true;
                await Connect(agentIpAddress, agentPort, username);
                _isReconnecting = false;
            }

            RequestData(agentIpAddress, agentPort, username);
        },
        null,
        TimeSpan.Zero,
        TimeSpan.FromSeconds(intervalInSeconds));

        _timers.AddOrUpdate(serverKey, timer, (key, oldValue) => timer);

        await MaintainConnection(agentIpAddress, agentPort);

        Console.WriteLine($"Started sending requests to {agentIpAddress}:{agentPort}:{username} every {intervalInSeconds} seconds.");
    }

    async Task MaintainConnection(string agentIpAddress, int agentPort)
    {
        string serverKey = $"{agentIpAddress}:{agentPort}:monitor";

        var keepAliveTimer = new Timer(async _ =>
        {
            RunCmd(agentIpAddress, agentPort, "ping");
        },
        null,
        TimeSpan.FromSeconds(20),//connection timeouts after 30s, so gotta send empty commands
        TimeSpan.FromSeconds(20));

        _timers.AddOrUpdate($"{serverKey}-keepalive", keepAliveTimer, (key, oldValue) => keepAliveTimer);

        Console.WriteLine($"{DateTime.Now} Sent a ping to server.");
    }

    public void ChangeInterval(string agentIpAddress, int agentPort, string username, int newIntervalInSeconds)
    {
        string serverKey = $"{agentIpAddress}:{agentPort}:{username}";
        if (_timers.TryGetValue(serverKey, out Timer timer))
        {
            _currentIntervals.AddOrUpdate(serverKey, newIntervalInSeconds, (key, oldValue) => newIntervalInSeconds);
            timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(newIntervalInSeconds));
            Console.WriteLine($"Interval for {agentIpAddress}:{agentPort}:{username} changed to {newIntervalInSeconds} seconds.");
        }
    }

    public void Disconnect(string agentIpAddress, int agentPort, string username)
    {
        string serverKey = $"{agentIpAddress}:{agentPort}:{username}";
        if (_timers.TryRemove(serverKey, out Timer timer))
        {
            timer?.Dispose();
        }

        if (_sshClients.TryRemove(serverKey, out SshClient sshClient))
        {
            sshClient?.Disconnect();
            sshClient?.Dispose();
            Console.WriteLine($"SSH Client for {agentIpAddress}:{agentPort}:{username} Disconnected.");
        }
    }

    public void DisconnectAll()
    {
        foreach (var serverKey in _sshClients.Keys)
        {
            var parts = serverKey.Split(':');
            Disconnect(parts[0], int.Parse(parts[1]), parts[2]);
        }
    }
}