
namespace monitor
{
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
        public PartitionMetrics(PartitionData t1, PartitionData t2)
        {

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
    }
}