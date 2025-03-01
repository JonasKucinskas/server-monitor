using Renci.SshNet;
using Renci.SshNet.Common;
using System;

public class SshConnection
{
    public static void ConnectToAgent(string _agentIpAddress, int _agentPort, string _username)
    {
        try
        {
            var privateKeyFile = new PrivateKeyFile("privateKey.pem");
            var authenticationMethod = new PrivateKeyAuthenticationMethod(_username, privateKeyFile);

            var connectionInfo = new Renci.SshNet.ConnectionInfo(_agentIpAddress, _agentPort, "monitor", authenticationMethod);

            using (var sshClient = new SshClient(connectionInfo))
            {
                sshClient.Connect();

                using SshCommand cmd = sshClient.RunCommand("echo 'dataRequest'");
                Console.WriteLine(cmd.Result); // "Hello World!\n"
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting metrics: {ex.Message}");
        }
    }
}
