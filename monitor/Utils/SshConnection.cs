using System.Diagnostics;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using FxSsh;
using FxSsh.Messages.Userauth;
using FxSsh.Services;
using Newtonsoft.Json;
using Org.BouncyCastle.Tls;

public class SshConnection
{
    private static string _hubPublicKey;

    public static void StartServer(string pub, string _agentPrivateKeyPath, int port)
    {
        _hubPublicKey = pub;

        StartingInfo info = new StartingInfo(IPAddress.Any, port, "SSH-2.0-Welcome");
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
            Console.WriteLine("Keys dont match, not authenticating");
            e.Result = false;
        }
    }

    static async void service_CommandOpenedAsync(object sender, CommandRequestedArgs e)
    {
        Console.WriteLine($"Channel {e.Channel.ServerChannelId} runs {e.ShellType}: \"{e.CommandText}\", client key SHA256:{e.AttachedUserauthArgs.Fingerprint}.");
        
        //e.Agreed = true;  // func(e.ShellType, e.CommandText, e.AttachedUserAuthArgs);

        //if (!e.Agreed)
        //    return;

        Console.WriteLine("Command received: " + e.CommandText);

        
        if (e.CommandText == "ping")
        {
            e.Channel.SendData(System.Text.Encoding.UTF8.GetBytes("ping"));
            e.Channel.SendClose();
            return;
        }
        else if (e.CommandText == "echo 'dataRequest'")
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
        else
        {
            //exec command 

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash", 
                Arguments = $"-c \"{e.CommandText}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();

                Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                Task<string> errorTask = process.StandardError.ReadToEndAsync();

                bool exited = process.WaitForExit(10000);

                if (!exited)
                {
                    Console.WriteLine("Process did not exit in 5 seconds. Killing...");
                    process.Kill();
                    process.WaitForExit();
                }

                string output = outputTask.Result;
                string error = errorTask.Result;

                Console.WriteLine("Output: " + output);
                e.Channel.SendData(System.Text.Encoding.UTF8.GetBytes(output));
                e.Channel.SendClose();
            }
        }
    }
}