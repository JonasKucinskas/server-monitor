using System.Text;
using System.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

class Program
{
    static async Task Main()
    {
        if (!File.Exists("privateKey.pem"))
        {
            KeyGen.GenerateKeyPair(); 
        }
        SshConnection.StartServer("publicKey.pub", "privateKey.pem");
        Sampler.Instance.BeginSampling();
        await Task.Delay(Timeout.Infinite);
    }
}
