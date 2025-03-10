using System;
using System.ComponentModel.Design.Serialization;
using System.Threading;
using Renci.SshNet;
public class SshConnection
{
    private readonly string _agentIpAddress;
    private readonly int _agentPort;
    private readonly string _username;
    private SshClient _sshClient;
    private Timer _timer;
    private int _currentIntervalInSeconds;
    private readonly DatabaseQueue<DataPackage> _databaseQueue;

    public SshConnection(string agentIpAddress, int agentPort, string username)
    {
        _agentIpAddress = agentIpAddress;
        _agentPort = agentPort;
        _username = username;
        _databaseQueue = new DatabaseQueue<DataPackage>(async (data) =>
        {
            var repo = new Database("Host=localhost;Port=5432;Username=postgres;Password=password;Database=monitor");
            await repo.InsertServerMetricsAsync(data); 
        });
    }

    public async Task Connect()
    {
        while (true)
        {
            try
            {
                var privateKeyFile = new PrivateKeyFile("privateKey.pem");
                var authenticationMethod = new PrivateKeyAuthenticationMethod(_username, privateKeyFile);
                var connectionInfo = new Renci.SshNet.ConnectionInfo(_agentIpAddress, _agentPort, "monitor", authenticationMethod);

                _sshClient = new SshClient(connectionInfo);
                _sshClient.Connect();

                Console.WriteLine("Connected to SSH server!");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now}: Error connecting to server: {ex.Message}. Retrying in 5 seconds...");
            }

            await Task.Delay(5000); 
        }
    }


    private void RequestData()
    {
        try
        {
            using (SshCommand cmd = _sshClient.RunCommand("echo 'dataRequest'"))
            {
                DataPackage data = Deserealizer.Deserealize(cmd.Result);
                _databaseQueue.Enqueue(data);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now}: Error requesting data: {ex.Message}");
        }
    }

    public async Task StartSendingRequests(int intervalInSeconds)
    {
        _currentIntervalInSeconds = intervalInSeconds;
        bool _isReconnecting = false;

        _timer = new Timer(async _ =>
        {
            if (_isReconnecting) return;
            if (_sshClient == null || !_sshClient.IsConnected)
            {
                _isReconnecting = true;
                await Connect();
                _isReconnecting = false;
            }

            RequestData();
        }, 
        null,
        TimeSpan.FromSeconds(_currentIntervalInSeconds), 
        TimeSpan.FromSeconds(_currentIntervalInSeconds));

        Console.WriteLine($"Started sending requests every {_currentIntervalInSeconds} seconds.");
    }

    public void ChangeInterval(int newIntervalInSeconds)
    {
        if (_timer != null)
        {
            _currentIntervalInSeconds = newIntervalInSeconds;
            _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(_currentIntervalInSeconds));
            Console.WriteLine($"Interval changed to {newIntervalInSeconds} seconds.");
        }
    }

    public void Disconnect()
    {
        _timer?.Dispose();
        _sshClient?.Disconnect();
        _sshClient?.Dispose();
        Console.WriteLine("SSH Client Disconnected.");
    }
}