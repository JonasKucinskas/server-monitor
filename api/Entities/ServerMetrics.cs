using monitor;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public class ServerMetrics
{
    public DateTime Time { get; set; }
    public string ServerId { get; set; }
    public string CpuName { get; set; }
    public double CpuFreq { get; set; }
    public string BatteryName { get; set; }
    public int BatteryCapacity { get; set; }
    public string BatteryStatus { get; set; }
    public long DiskTotalSpace { get; set; }
    public long DiskUsedSpace { get; set; }
    public long DiskFreeSpace { get; set; }
    public long RamMemTotal { get; set; }
    public long RamMemFree { get; set; }
    public long RamMemUsed { get; set; }
    public long RamMemAvailable { get; set; }
    public long RamBuffers { get; set; }
    public long RamCached { get; set; }
    public long RamSwapTotal { get; set; }
    public long RamSwapFree { get; set; }
    public long RamSwapUsed { get; set; }
    public List<Sensor> SensorList { get; set; } = new();
    public List<CoreMetrics> CpuCores { get; set; } = new();
    public List<DiskPartition> DiskPartitions { get; set; } = new();
    public List<NetworkMetric> NetworkMetrics { get; set; } = new();

    public async Task InsertToDatabase(NpgsqlConnection conn)
    {
        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO server_metrics (time, server_id, cpu_name, cpu_freq, battery_name, battery_capacity, battery_status, disk_total_space, disk_used_space, disk_free_space, ram_mem_total, ram_mem_free, ram_mem_used, ram_mem_available, ram_buffers, ram_cached, ram_swap_total, ram_swap_free, ram_swap_used)
            VALUES (@time, @serverId, @cpuName, @cpuFreq, @batteryName, @batteryCapacity, @batteryStatus, @diskTotalSpace, @diskUsedSpace, @diskFreeSpace, @ramMemTotal, @ramMemFree, @ramMemUsed, @ramMemAvailable, @ramBuffers, @ramCached, @ramSwapTotal, @ramSwapFree, @ramSwapUsed);
        ", conn);

        cmd.Parameters.AddWithValue("time", Time);
        cmd.Parameters.AddWithValue("serverId", ServerId);
        cmd.Parameters.AddWithValue("cpuName", CpuName);
        cmd.Parameters.AddWithValue("cpuFreq", CpuFreq);
        cmd.Parameters.AddWithValue("batteryName", BatteryName);
        cmd.Parameters.AddWithValue("batteryCapacity", BatteryCapacity);
        cmd.Parameters.AddWithValue("batteryStatus", BatteryStatus);
        cmd.Parameters.AddWithValue("diskTotalSpace", DiskTotalSpace);
        cmd.Parameters.AddWithValue("diskUsedSpace", DiskUsedSpace);
        cmd.Parameters.AddWithValue("diskFreeSpace", DiskFreeSpace);
        cmd.Parameters.AddWithValue("ramMemTotal", RamMemTotal);
        cmd.Parameters.AddWithValue("ramMemFree", RamMemFree);
        cmd.Parameters.AddWithValue("ramMemUsed", RamMemUsed);
        cmd.Parameters.AddWithValue("ramMemAvailable", RamMemAvailable);
        cmd.Parameters.AddWithValue("ramBuffers", RamBuffers);
        cmd.Parameters.AddWithValue("ramCached", RamCached);
        cmd.Parameters.AddWithValue("ramSwapTotal", RamSwapTotal);
        cmd.Parameters.AddWithValue("ramSwapFree", RamSwapFree);
        cmd.Parameters.AddWithValue("ramSwapUsed", RamSwapUsed);

        await cmd.ExecuteNonQueryAsync();

        foreach (var core in CpuCores)
        {
            await core.InsertToDatabase(conn, Time, ServerId);
        }

        foreach (var partition in DiskPartitions)
        {
            await partition.InsertToDatabase(conn, Time, ServerId);
        }

        foreach (var network in NetworkMetrics)
        {
            await network.InsertToDatabase(conn, Time, ServerId);
        }

        foreach (var sensor in SensorList)
        {
            await sensor.InsertToDatabase(conn, Time, ServerId);
        }

        Console.WriteLine(DateTime.Now + ": Data inserted into the database");
    }
}
