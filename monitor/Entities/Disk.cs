using Mono.Unix;

public class Disk
{
    public long TotalSpace;
    public long UsedSpace;
    public long FreeSpace;
    public List<PartitionMetrics> Partitions;
    public Disk()
    {
        SetDiskStats();
        Partitions = [];
    }

    public void ClearMetrics()
    {
        Partitions = [];
    }

    public Disk Clone()
    {
        return new Disk
        {
            TotalSpace = this.TotalSpace,
            UsedSpace = this.UsedSpace,
            FreeSpace = this.FreeSpace,
            Partitions = this.Partitions
        };
    }

    public void SetDiskStats()
    {
        UnixDriveInfo drive = new UnixDriveInfo("/");
        this.TotalSpace = drive.TotalSize;
        this.FreeSpace = drive.AvailableFreeSpace;
        this.UsedSpace = TotalSpace - FreeSpace;
    }

    public async Task GatherMetrics()
    {
        var sample1 = PartitionData.SampleNetworkData();
        await Task.Delay(1000);
        var sample2 = PartitionData.SampleNetworkData();
        foreach (var kv in sample1)
        {
            string partitionName = kv.Key;
            if (!sample2.ContainsKey(partitionName))
            {
                continue; // Skip if for some reason the second sample doesn't have this core.
            }
            PartitionData t1 = sample1[partitionName];
            PartitionData t2 = sample2[partitionName];
            
            this.Partitions.Add(new(t1, t2));
        }
    }
}

public class PartitionMetrics
{
    public string Name;
    public long ReadSpeed;
    public long WriteSpeed;
    public long IoTime;
    public long WeightedIoTime;
    public PartitionMetrics(PartitionData t1, PartitionData t2)
    {
        this.Name = t1.Name;
        this.ReadSpeed = (t2.ReadSectors - t1.ReadSectors) * 512 / 1;
        this.WriteSpeed = (t2.WriteSectors - t1.WriteSectors) * 512 / 1;
        this.IoTime = (t2.IoTime - t1.IoTime) / 2;
        this.WeightedIoTime = (t2.WeightedIoTime - t1.WeightedIoTime) / 2;
    }

    public PartitionMetrics()
    {
    }
}

public class PartitionData
{
    public string Name {get; private set;}
    public long Reads {get; private set;}
    public long ReadSectors {get; private set;}
    public long Writes {get; private set;}
    public long WriteSectors {get; private set;}
    public long IoTime {get; private set;}
    public long WeightedIoTime {get; private set;}

    public static Dictionary<string, PartitionData> SampleNetworkData()
    {
        var partitions = new Dictionary<string, PartitionData>();
        foreach (var line in File.ReadLines("/proc/diskstats"))
        {
            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var partition = new PartitionData
            {
                Name = parts[2], 
                Reads = long.Parse(parts[3]), 
                ReadSectors = long.Parse(parts[5]),
                Writes = long.Parse(parts[7]),
                WriteSectors = long.Parse(parts[9]), 
                IoTime = long.Parse(parts[12]),
                WeightedIoTime = long.Parse(parts[13]) 
            };

            partitions[parts[2]] = partition;
        }

        return partitions;
    }
}
