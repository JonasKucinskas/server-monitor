public class Network
{
    public List<NetworkMetric> Metrics { get; set; }
    public Network()
    {
        Metrics = [];
    }
    public void ClearMetrics()
    {
        Metrics = [];
    }
    public Network Clone()
    {
        return new Network
        {
            Metrics = this.Metrics
        };
    }
    public async Task GatherMetrics()
    {
        var sample1 = NetworkInterfaceData.SampleNetworkData();
        await Task.Delay(1000);
        var sample2 = NetworkInterfaceData.SampleNetworkData();
        foreach (var kv in sample1)
        {
            string coreName = kv.Key;
            if (!sample2.ContainsKey(coreName))
            {
                continue; 
            }
            NetworkInterfaceData t1 = sample1[coreName];
            NetworkInterfaceData t2 = sample2[coreName];
            
            this.Metrics.Add(new(t1, t2));
        }
    }
}
public class NetworkMetric
{
    //b/s
    public string Name;
    public long Upload;
    public long Download;
    public NetworkInterfaceData iface;
    public NetworkMetric()
    {
        
    }
    public NetworkMetric(NetworkInterfaceData t1, NetworkInterfaceData t2)
    {
        this.Name = t1.Name;
        this.Upload = t2.TransmitBytes - t1.TransmitBytes;
        this.Download =  t2.ReceiveBytes - t1.ReceiveBytes;
        //
        this.iface = t2;
    }
}
public class NetworkInterfaceData
{
    public string Name { get; private set; }
    
    // Receive columns
    public long ReceiveBytes { get; private set; }
    public long ReceivePackets { get; private set; }
    public long ReceiveErrs { get; private set; }
    public long ReceiveDrop { get; private set; }
    public long ReceiveFifo { get; private set; }
    public long ReceiveFrame { get; private set; }
    public long ReceiveCompressed { get; private set; }
    public long ReceiveMulticast { get; private set; }
    
    // Transmit columns
    public long TransmitBytes { get; private set; }
    public long TransmitPackets { get; private set; }
    public long TransmitErrs { get; private set; }
    public long TransmitDrop { get; private set; }
    public long TransmitFifo { get; private set; }
    public long TransmitColls { get; private set; }
    public long TransmitCarrier { get; private set; }
    public long TransmitCompressed { get; private set; }

    public static Dictionary<string, NetworkInterfaceData> SampleNetworkData()
    {
        Dictionary<string, NetworkInterfaceData> ifaces = [];
        var lines = File.ReadAllLines("/proc/net/dev");
        
        foreach (var line in lines.Skip(2))
        {
            var parts = line.Split(':');
            
            if (parts.Length != 2)
                continue;
            
            var ifaceName = parts[0].Trim();
            
            var data = parts[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (data.Length < 16)
                continue;
            var ifaceStats = new NetworkInterfaceData
            {
                Name = ifaceName,
                
                ReceiveBytes = long.Parse(data[0]),
                ReceivePackets = long.Parse(data[1]),
                ReceiveErrs = long.Parse(data[2]),
                ReceiveDrop = long.Parse(data[3]),
                ReceiveFifo = long.Parse(data[4]),
                ReceiveFrame = long.Parse(data[5]),
                ReceiveCompressed = long.Parse(data[6]),
                ReceiveMulticast = long.Parse(data[7]),

                TransmitBytes = long.Parse(data[8]),
                TransmitPackets = long.Parse(data[9]),
                TransmitErrs = long.Parse(data[10]),
                TransmitDrop = long.Parse(data[11]),
                TransmitFifo = long.Parse(data[12]),
                TransmitColls = long.Parse(data[13]),
                TransmitCarrier = long.Parse(data[14]),
                TransmitCompressed = long.Parse(data[15])
            };
            ifaces[ifaceName] = ifaceStats;
        }
        return ifaces;
    }
}