public class Disk
{
    public long TotalSpace;
    public long UsedSpace;
    public long FreeSpace;
    public List<PartitionMetrics> Partitions;
}
public class PartitionMetrics
{
    public string Name;
    public long ReadSpeed;
    public long WriteSpeed;
    public long IoTime;
    public long WeightedIoTime;
}