using Microsoft.Win32;

public class Database
{
    private readonly string connectionString;
    public Database(string connectionString)
    {
        this.connectionString = connectionString;
    }


    public async Task InsertServerMetricsAsync(DataPackage metrics)
    {
        await ConvertToDataBaseObjects(metrics).InsertToDatabase(connectionString);
    }

    private static ServerMetrics ConvertToDataBaseObjects(DataPackage data)
    {
        ServerMetrics serverMetrics = new ServerMetrics()
        {
            Time = DateTime.Now,
            ServerId = "localhost",//TODO 
            CpuName = data.Cpu.Name,
            CpuFreq = data.Cpu.Freq,
            BatteryName = data.Battery.Name,
            BatteryCapacity = data.Battery.Capacity,
            BatteryStatus = data.Battery.Status,
            DiskTotalSpace = data.Disk.TotalSpace,
            DiskUsedSpace = data.Disk.UsedSpace,
            DiskFreeSpace = data.Disk.FreeSpace,
            RamMemTotal = data.Ram.MemTotal,
            RamMemFree = data.Ram.MemFree,
            RamMemUsed = data.Ram.MemUsed,
            RamMemAvailable = data.Ram.MemAvailable,
            RamBuffers = data.Ram.Buffers,
            RamCached = data.Ram.Cached,
            RamSwapTotal = data.Ram.SwapTotal,
            RamSwapFree = data.Ram.SwapFree,
            RamSwapUsed = data.Ram.SwapUsed
        };
        
        serverMetrics.CpuCores = new List<CpuCore>();

        foreach(var dataPoint in data.Cpu.Cores)
        {
            var cpuCore = new CpuCore()
            {
                CoreName = dataPoint.CoreName,
                Total = dataPoint.Total,
                User = dataPoint.User,
                Nice = dataPoint.Nice,
                System = dataPoint.System,
                Idle = dataPoint.Idle,
                IOWait = dataPoint.IOWait,
                IRQ = dataPoint.IRQ,
                SoftIRQ = dataPoint.SoftIRQ,
                Steal = dataPoint.Steal
            };

            serverMetrics.CpuCores.Add(cpuCore);
        }

        serverMetrics.NetworkMetrics = new List<NetworkMetric>();

        foreach(var dataPoint in data.Network.Metrics)
        {
            var networkMetric = new NetworkMetric()
            {
                Name = dataPoint.Name, 
                Upload = dataPoint.Upload, 
                Download = dataPoint.Download, 
                Iface = dataPoint.Iface
            };

            serverMetrics.NetworkMetrics.Add(networkMetric);
        }

        serverMetrics.DiskPartitions = new List<DiskPartition>();

        foreach(var dataPoint in data.Disk.Partitions)
        {
            var diskPartition = new DiskPartition()
            {
                Name = dataPoint.Name,
                ReadSpeed = dataPoint.ReadSpeed,
                WriteSpeed = dataPoint.WriteSpeed,
                IoTime = dataPoint.IoTime,
                WeightedIoTime = dataPoint.WeightedIoTime
            };
            serverMetrics.DiskPartitions.Add(diskPartition);
        }

        serverMetrics.SensorList = new SensorList();

        foreach(var dataPoint in data.SensorList.Sensors)
        {
            var sensor = new Sensor()
            {
                Name = dataPoint.Name,
                value = dataPoint.value
            };

            serverMetrics.SensorList.Sensors.Add(sensor);
        }

        return serverMetrics;
    }
}