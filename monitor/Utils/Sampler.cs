using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class Sampler
{   
    private static bool isSampling = false;
    private static Sampler _instance;
    private static readonly object _lock = new object();
    
    private List<DataPackage> samples;

    private Sampler()
    {
        samples = new List<DataPackage>();
    }

    public static Sampler Instance
    {
        get
        {
            lock (_lock)
            {
                return _instance ??= new Sampler();
            }
        }
    }

    public void ClearSamples()
    {   

        samples = new List<DataPackage>();
    }

    public List<DataPackage> GetSamples()
    {
        return samples;
    }

    public void BeginSampling()
    {
        if (isSampling)
        {
            Console.WriteLine("Already sampling");
            return;
        }

        isSampling = false;

        Cpu cpu = new();
        Ram ram = new();
        Network network = new();
        Disk disk = new();
        Battery battery = new();
        SensorList sensors = new();

        var periodicTask = new TimedTask(async () =>
        {
            Console.WriteLine($"Collecting sample: {DateTime.Now}");

            await Task.WhenAll(
                cpu.GatherMetrics(),
                network.GatherMetrics(),
                disk.GatherMetrics()
            );

            var ramTask = Task.Run(() => ram.GatherMetrics());
            var batteryTask = Task.Run(() => battery.GatherMetrics());
            var sensorsTask = Task.Run(() => sensors.GatherMetrics());

            await Task.WhenAll(ramTask, batteryTask, sensorsTask);

            DataPackage data = new DataPackage
            {
                Cpu = cpu.Clone(),
                Ram = ram.Clone(),
                Network = network.Clone(),
                Disk = disk.Clone(),
                Battery = battery.Clone(),
                SensorList = sensors.Clone()
            };

            samples.Add(data);

            await Task.WhenAll(
                Task.Run(() => cpu.ClearMetrics()),
                Task.Run(() => network.ClearMetrics()),
                Task.Run(() => disk.ClearMetrics()),
                Task.Run(() => sensors.ClearMetrics())
            );
        }, TimeSpan.FromSeconds(1));

        periodicTask.Start();
    }
}