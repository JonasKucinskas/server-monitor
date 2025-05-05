using System.Text;
using System.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using System.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {

        var publicKeyOption = new Option<string>(
            name: "-k",
            getDefaultValue: () => "");

        var portOption = new Option<int>(
            name: "-p",
            getDefaultValue: () => 12345);

        var rootCommand = new RootCommand("Your app description")
        {
            publicKeyOption,
            portOption
        };

        rootCommand.SetHandler(async (string key, int port) =>
        {
            Console.WriteLine($"Using key: {key}");
            Console.WriteLine($"Using port: {port}");

            if (!File.Exists("privateKey.pem"))
            {
                KeyGen.GenerateKeyPair();
            }

            SshConnection.StartServer(key, "privateKey.pem", port);
            Sampler.Instance.BeginSampling();

            await Task.Delay(Timeout.Infinite);
        }, publicKeyOption, portOption);

        return await rootCommand.InvokeAsync(args);
    }
}
