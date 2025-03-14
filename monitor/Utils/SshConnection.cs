using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using FxSsh;
using FxSsh.Messages.Userauth;
using FxSsh.Services;
using Monitor;
using Newtonsoft.Json;
using Org.BouncyCastle.Tls;

namespace monitor;

public class SshConnection
{
    private static string _hubPublicKey;
    private static string _agentPrivateKeyPath; 

    public static void StartServer(string pub, string _agentPrivateKeyPath)
    {
        _hubPublicKey = File.ReadAllText(pub);

        StartingInfo info = new StartingInfo(IPAddress.Any, 12345, "SSH-2.0-Welcome");
        var server = new SshServer(info);

        var agentPrivateKey = File.ReadAllText(_agentPrivateKeyPath);
        server.AddHostKey("rsa-sha2-256", KeyGenerator.GenerateRsaKeyPem(2048));

        server.ConnectionAccepted += server_ConnectionAccepted;
        server.ExceptionRasied += server_ConnectionFailed;

        server.Start();

        Console.WriteLine("Agent is running, listening for connections...");
    }
    
    static void server_ConnectionFailed(object sender, Exception e)
    {
        Console.WriteLine(e.Message);
    } 

    static void server_ConnectionAccepted(object sender, Session e)
    {
        Console.WriteLine("Accepted a client.");

        e.ServiceRegistered += e_ServiceRegistered;
        e.KeysExchanged += e_KeysExchanged;
    }   

    private static void e_KeysExchanged(object sender, KeyExchangeArgs e)
    {
        foreach (var keyExchangeAlg in e.KeyExchangeAlgorithms)
        {
            Console.WriteLine("Key exchange algorithm: {0}", keyExchangeAlg);
        }
    }

    static void e_ServiceRegistered(object sender, SshService e)
    {
        var session = (Session)sender;
        Console.WriteLine("Session {0} requesting {1}.",
        BitConverter.ToString(session.SessionId).Replace("-", ""), e.GetType().Name);

        if (e is UserauthService)
        {
            var service = (UserauthService)e;
            service.Userauth += service_UserAuth;
        }
        else if (e is ConnectionService)
        {
            var service = (ConnectionService)e;
            service.CommandOpened += service_CommandOpenedAsync;
            service.EnvReceived += service_EnvReceived;

        }
    }

    static void service_EnvReceived(object sender, EnvironmentArgs e)
    {
        Console.WriteLine("Received environment variable {0}:{1}", e.Name, e.Value);
    }

    static void service_UserAuth(object sender, UserauthArgs e)
    {
        //Console.WriteLine("Client {0} fingerprint: {1} key {2}.", e.KeyAlgorithm, e.Fingerprint, e.Key);
        string clientPublicKey = Convert.ToBase64String(e.Key);

        if (_hubPublicKey == clientPublicKey)
        {
            Console.WriteLine("Authenticated");
            e.Result = true;
        }
        else
        {
            Console.WriteLine("Keys dont match, not authenticting");
            e.Result = false;
        }
    }

    static async void service_CommandOpenedAsync(object sender, CommandRequestedArgs e)
    {
        Console.WriteLine($"Channel {e.Channel.ServerChannelId} runs {e.ShellType}: \"{e.CommandText}\", client key SHA256:{e.AttachedUserauthArgs.Fingerprint}.");
        
        //e.Agreed = true;  // func(e.ShellType, e.CommandText, e.AttachedUserAuthArgs);

        if (e.CommandText == "echo 'dataRequest'")
        {
            List<DataPackage> samples = Sampler.Instance.GetSamples();        

            var cpuTask = Task.Run(() => AverageCalculator.Cpu(samples));
            var batteryTask = Task.Run(() => AverageCalculator.Battery(samples));
            var diskTask = Task.Run(() => AverageCalculator.Disk(samples));
            var networkTask = Task.Run(() => AverageCalculator.Network(samples));
            var ramTask = Task.Run(() => AverageCalculator.Ram(samples));
            var sensorListTask = Task.Run(() => AverageCalculator.SensorList(samples));

            await Task.WhenAll(cpuTask, batteryTask, diskTask, networkTask, ramTask, sensorListTask);

            DataPackage package = new DataPackage
            {
                Cpu = cpuTask.Result,
                Battery = batteryTask.Result,
                Disk = diskTask.Result,
                Network = networkTask.Result,
                Ram = ramTask.Result,
                SensorList = sensorListTask.Result
            };

            string json = JsonConvert.SerializeObject(package);
            
            //Console.WriteLine(json);
            
            try
            {
                Console.WriteLine("Sending data");
                e.Channel.SendData(System.Text.Encoding.UTF8.GetBytes(json));
                e.Channel.SendClose();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Sampler.Instance.ClearSamples();
            Console.WriteLine("Samples Cleared");
        }

        /*
        if (!e.Agreed)
            return;

        if (e.ShellType == "shell")
        {
            // requirements: Windows 10 RedStone 5, 1809
            // also, you can call powershell.exe
            var terminal = new Terminal("cmd.exe", windowWidth, windowHeight);

            e.Channel.DataReceived += (ss, ee) => terminal.OnInput(ee);
            e.Channel.CloseReceived += (ss, ee) => terminal.OnClose();
            terminal.DataReceived += (ss, ee) => e.Channel.SendData(ee);
            terminal.CloseReceived += (ss, ee) => e.Channel.SendClose(ee);

            terminal.Run();
        }
        else if (e.ShellType == "exec")
        {
            var parser = new Regex(@"(?<cmd>git-receive-pack|git-upload-pack|git-upload-archive) \'/?(?<proj>.+)\.git\'");
            var match = parser.Match(e.CommandText);
            var command = match.Groups["cmd"].Value;
            var project = match.Groups["proj"].Value;

            var git = new GitService(command, project);

            e.Channel.DataReceived += (ss, ee) => git.OnData(ee);
            e.Channel.CloseReceived += (ss, ee) => git.OnClose();
            git.DataReceived += (ss, ee) => e.Channel.SendData(ee);
            git.CloseReceived += (ss, ee) => e.Channel.SendClose(ee);

            git.Start();
        }
        else if (e.ShellType == "subsystem")
        {
            if (e.CommandText == "sftp")
            {
                var sftp = new SftpService(OperatingSystem.IsWindows() ? @"C:\" : @"/");
                e.Channel.DataReceived += (ss, ee) => sftp.OnData(ee);
                e.Channel.CloseReceived += (ss, ee) => sftp.OnClose();
                sftp.DataReceived += (ss, ee) => e.Channel.SendData(ee);
            }
        }
        */
    }
    public class MyData
    {
        public string Name;
        public int Value;
    }
}