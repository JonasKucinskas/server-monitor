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
            var repo = new Database("Host=localhost;Port=5432;Username=postgres;Password=password;Database=postgres;");
            await repo.InsertServerMetricsAsync(data); 
        });
    }

    public void Connect()
    {
        try
        {
            var privateKeyFile = new PrivateKeyFile("privateKey.pem");
            var authenticationMethod = new PrivateKeyAuthenticationMethod(_username, privateKeyFile);
            var connectionInfo = new Renci.SshNet.ConnectionInfo(_agentIpAddress, _agentPort, "monitor", authenticationMethod);

            _sshClient = new SshClient(connectionInfo);
            _sshClient.Connect();

            Console.WriteLine("SSH Client Connected!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to client: {ex.Message}");
        }
    }

    private void RequestData()
    {
        try
        {
            if (_sshClient != null && _sshClient.IsConnected)
            {
                using (SshCommand cmd = _sshClient.RunCommand("echo 'dataRequest'"))
                {
                    //deserealise to original data structure

                    DataPackage data = Deserealizer.Deserealize(cmd.Result);
                    //Console.WriteLine(data.SensorList.Sensors[0].Name); 
                    //Console.WriteLine(data.SensorList.Sensors[0].value); 
                    
                    _databaseQueue.Enqueue(data);
                }
            }
            else
            {
                Console.WriteLine("SSH Client is not connected!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error requesting data: {ex.Message}");
        }
    }

    public void StartSendingRequests(int intervalInSeconds)
    {
        _currentIntervalInSeconds = intervalInSeconds;

        if (_sshClient == null || !_sshClient.IsConnected)
        {
            Console.WriteLine("client not connected to server");
            return;
        }

        _timer = new Timer(
            e => RequestData(),
            null,
            TimeSpan.FromSeconds(_currentIntervalInSeconds), 
            TimeSpan.FromSeconds(_currentIntervalInSeconds) 
        );

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