using Renci.SshNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public sealed class MultiSshConnection
{
    private static readonly Lazy<MultiSshConnection> _instance = new Lazy<MultiSshConnection>(() => new MultiSshConnection());
    public static MultiSshConnection Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, SshClient> _sshClients = new ConcurrentDictionary<string, SshClient>();
    private readonly ConcurrentDictionary<string, Timer> _timers = new ConcurrentDictionary<string, Timer>();
    private readonly ConcurrentDictionary<string, int> _currentIntervals = new ConcurrentDictionary<string, int>();
    private readonly DatabaseQueue<DataPackage> _databaseQueue;

    private MultiSshConnection()
    {
        _databaseQueue = new DatabaseQueue<DataPackage>(async (data) =>
        {
            var repo = new Database("Host=localhost;Port=5432;Username=postgres;Password=password;Database=monitor");
            await repo.InsertServerMetricsAsync(data);
        });
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
                Console.WriteLine($"{DateTime.Now}: Error running command on {agentIpAddress}:{port}:{"monitor"}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"{DateTime.Now}: SSH client for {serverKey} not found.");
        }

        return null;
    }

    private void RequestData(string agentIpAddress, int agentPort, string username)
    {
        string serverKey = $"{agentIpAddress}:{agentPort}:{username}";
        if (_sshClients.TryGetValue(serverKey, out SshClient sshClient))
        {
            try
            {
                using (SshCommand cmd = sshClient.RunCommand("echo 'dataRequest'"))
                {
                    Console.WriteLine(serverKey);
                    DataPackage data = Deserealizer.Deserealize(cmd.Result);
                    _databaseQueue.Enqueue(data);
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