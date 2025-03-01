using System.Runtime.CompilerServices;
using Monitor;

namespace monitor;

public static class AverageCalculator
{
    public static Cpu Cpu(List<DataPackage> dataPackages)
    {
        if (dataPackages == null || dataPackages.Count == 0)
        {
            return new Cpu(); 
        }

        Cpu AvgCpu = new Cpu
        {
            Name = dataPackages[0].Cpu.Name,
            Freq = dataPackages[0].Cpu.Freq
        };

        Dictionary<string, CoreMetrics> coreAverages = new Dictionary<string, CoreMetrics>();

        foreach (DataPackage package in dataPackages)
        {
            foreach (CoreMetrics core in package.Cpu.Cores)
            {
                if (!coreAverages.ContainsKey(core.CoreName))
                {
                    coreAverages[core.CoreName] = new CoreMetrics { CoreName = core.CoreName };
                }

                coreAverages[core.CoreName].Total += core.Total;
                coreAverages[core.CoreName].User += core.User;
                coreAverages[core.CoreName].Nice += core.Nice;
                coreAverages[core.CoreName].System += core.System;
                coreAverages[core.CoreName].Idle += core.Idle;
                coreAverages[core.CoreName].IOWait += core.IOWait;
                coreAverages[core.CoreName].IRQ += core.IRQ;
                coreAverages[core.CoreName].SoftIRQ += core.SoftIRQ;
                coreAverages[core.CoreName].Steal += core.Steal;
            }
        }

        int packageCount = dataPackages.Count;

        foreach (var coreAverage in coreAverages.Values)
        {
            coreAverage.Total /= packageCount;
            coreAverage.User /= packageCount;
            coreAverage.Nice /= packageCount;
            coreAverage.System /= packageCount;
            coreAverage.Idle /= packageCount;
            coreAverage.IOWait /= packageCount;
            coreAverage.IRQ /= packageCount;
            coreAverage.SoftIRQ /= packageCount;
            coreAverage.Steal /= packageCount;

            AvgCpu.Cores.Add(coreAverage);
        }

        return AvgCpu;
    }

    public static Disk Disk(List<DataPackage> dataPackages)
    {
        if (dataPackages == null || dataPackages.Count == 0)
        {
            return new Disk(); 
        }

        Dictionary<string, PartitionMetrics> partitionAverages = new Dictionary<string, PartitionMetrics>();

        //latest data for these.
        Disk AvgDisk = new Disk()
        {
            TotalSpace = dataPackages.Last().Disk.TotalSpace,
            UsedSpace = dataPackages.Last().Disk.UsedSpace,
            FreeSpace = dataPackages.Last().Disk.FreeSpace
        };


        foreach (DataPackage package in dataPackages)
        {
            foreach (PartitionMetrics partition in package.Disk.Partitions)
            {
                if (!partitionAverages.ContainsKey(partition.Name))
                {
                    partitionAverages[partition.Name] = new PartitionMetrics { Name = partition.Name };
                }

                partitionAverages[partition.Name].ReadSpeed += partition.ReadSpeed;
                partitionAverages[partition.Name].WriteSpeed += partition.WriteSpeed;
                partitionAverages[partition.Name].IoTime += partition.IoTime;
                partitionAverages[partition.Name].WeightedIoTime += partition.WeightedIoTime;
            }
        }

        int packageCount = dataPackages.Count;

        foreach (var partitionAverage in partitionAverages.Values)
        {
            partitionAverage.ReadSpeed /= packageCount;
            partitionAverage.WriteSpeed /= packageCount;
            partitionAverage.IoTime /= packageCount;
            partitionAverage.WeightedIoTime /= packageCount;

            AvgDisk.Partitions.Add(partitionAverage);
        }

        return AvgDisk;
    }

    public static Battery Battery(List<DataPackage> dataPackages)
    {
        if (dataPackages == null || dataPackages.Count == 0)
        {
            return new Battery(); 
        }

        Battery battery = dataPackages.Last().Battery.Clone();
        battery.Capacity = 0;

        foreach (DataPackage package in dataPackages)
        {
            battery.Capacity += package.Battery.Capacity;
        } 

        battery.Capacity /= dataPackages.Count;

        return battery;
    }

    public static Network Network(List<DataPackage> dataPackages)
    {
        if (dataPackages == null || dataPackages.Count == 0)
        {
            return new Network(); 
        }

        Network network = new Network();

        Dictionary<string, NetworkMetric> networkAverages = new Dictionary<string, NetworkMetric>();

        foreach (DataPackage package in dataPackages)
        {
            foreach (NetworkMetric metric in package.Network.Metrics)
            {
                if (!networkAverages.ContainsKey(metric.Name))
                {
                    networkAverages[metric.Name] = new NetworkMetric { Name = metric.Name };
                }

                networkAverages[metric.Name].Download += metric.Download;
                networkAverages[metric.Name].Upload += metric.Upload;
                
                if (networkAverages[metric.Name].iface == null)
                {
                    networkAverages[metric.Name].iface = metric.iface;
                }
            }
        }

        int packageCount = dataPackages.Count;

        foreach (var networkAverage in networkAverages.Values)
        {
            networkAverage.Upload /= packageCount;
            networkAverage.Download /= packageCount;

            network.Metrics.Add(networkAverage);
        }

        return network;
    }

    public static Ram Ram(List<DataPackage> dataPackages)
    {
        if (dataPackages == null || dataPackages.Count == 0)
        {
            return new Ram(); 
        }

        Ram averageRam = new Ram
        {
            SwapTotal = dataPackages[0].Ram.SwapTotal,
            MemTotal = dataPackages[0].Ram.MemTotal
        };

        foreach (DataPackage package in dataPackages)
        {
            Ram ram = package.Ram;

            averageRam.MemFree += ram.MemFree;
            averageRam.MemAvailable += ram.MemAvailable;
            averageRam.Buffers += ram.Buffers;
            averageRam.Cached += ram.Cached;
            averageRam.SwapFree += ram.SwapFree;
        }

        int packageCount = dataPackages.Count;

        averageRam.MemFree /= packageCount;
        averageRam.MemAvailable /= packageCount;
        averageRam.Buffers /= packageCount;
        averageRam.Cached /= packageCount;
        averageRam.SwapFree /= packageCount;

        return averageRam;
    }

    public static SensorList SensorList(List<DataPackage> dataPackages)
    {
        if (dataPackages == null || dataPackages.Count == 0)
        {
            return new SensorList(); 
        }

        SensorList sensorList = new SensorList();

        Dictionary<string, Sensor> sensorsAverages = new Dictionary<string, Sensor>();

        foreach (DataPackage package in dataPackages)
        {
            foreach (Sensor sensor in package.SensorList.Sensors)
            {
                if (!sensorsAverages.ContainsKey(sensor.Name))
                {
                    sensorsAverages[sensor.Name] = new Sensor { Name = sensor.Name };
                }

                sensorsAverages[sensor.Name].value += sensor.value;
            }
        }

        int packageCount = dataPackages.Count;

        foreach (var sensorsAverage in sensorsAverages.Values)
        {
            sensorsAverage.value /= packageCount;

            sensorList.Sensors.Add(sensorsAverage);
        }

        return sensorList;
    }
}