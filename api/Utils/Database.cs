using System.Data.Common;
using Npgsql;
public class Database
{
    private readonly string connectionString;
    private readonly NpgsqlConnection conn;
    public Database(string connectionString)
    {
        this.connectionString = connectionString;
        this.conn = new NpgsqlConnection(connectionString);
    }

    public async Task<List<ServerMetrics>> FetchServerMetrics(string serverId, DateTime startTime, DateTime endTime)
    {
        if (conn.State != System.Data.ConnectionState.Open)
        {
            await conn.OpenAsync();
        }

        var serverMetricsList = new List<ServerMetrics>();

        string serverMetricsQuery = @"
            SELECT time, server_id, cpu_name, cpu_freq, battery_name, battery_capacity, battery_status,
                disk_total_space, disk_used_space, disk_free_space, ram_mem_total, ram_mem_free, ram_mem_used,
                ram_mem_available, ram_buffers, ram_cached, ram_swap_total, ram_swap_free, ram_swap_used
            FROM server_metrics
            WHERE server_id = @serverId
            AND time BETWEEN @startTime AND @endTime
            ORDER BY time;
        ";

        await using var cmd = new NpgsqlCommand(serverMetricsQuery, conn);
        cmd.Parameters.AddWithValue("serverId", serverId);
        cmd.Parameters.AddWithValue("startTime", startTime);
        cmd.Parameters.AddWithValue("endTime", endTime);

        await using var reader = await cmd.ExecuteReaderAsync();

        try
        {
            while (await reader.ReadAsync())
            {
                // Create a new ServerMetrics object for each row in the result
                var serverMetrics = new ServerMetrics
                {
                    Time = reader.GetDateTime(reader.GetOrdinal("time")),
                    ServerId = reader.GetString(reader.GetOrdinal("server_id")),
                    CpuName = reader.GetString(reader.GetOrdinal("cpu_name")),
                    CpuFreq = reader.GetDouble(reader.GetOrdinal("cpu_freq")),
                    BatteryName = reader.GetString(reader.GetOrdinal("battery_name")),
                    BatteryCapacity = reader.GetInt32(reader.GetOrdinal("battery_capacity")),
                    BatteryStatus = reader.GetString(reader.GetOrdinal("battery_status")),
                    DiskTotalSpace = reader.GetInt64(reader.GetOrdinal("disk_total_space")),
                    DiskUsedSpace = reader.GetInt64(reader.GetOrdinal("disk_used_space")),
                    DiskFreeSpace = reader.GetInt64(reader.GetOrdinal("disk_free_space")),
                    RamMemTotal = reader.GetInt64(reader.GetOrdinal("ram_mem_total")),
                    RamMemFree = reader.GetInt64(reader.GetOrdinal("ram_mem_free")),
                    RamMemUsed = reader.GetInt64(reader.GetOrdinal("ram_mem_used")),
                    RamMemAvailable = reader.GetInt64(reader.GetOrdinal("ram_mem_available")),
                    RamBuffers = reader.GetInt64(reader.GetOrdinal("ram_buffers")),
                    RamCached = reader.GetInt64(reader.GetOrdinal("ram_cached")),
                    RamSwapTotal = reader.GetInt64(reader.GetOrdinal("ram_swap_total")),
                    RamSwapFree = reader.GetInt64(reader.GetOrdinal("ram_swap_free")),
                    RamSwapUsed = reader.GetInt64(reader.GetOrdinal("ram_swap_used"))
                };

                // Add the current object to the list
                serverMetricsList.Add(serverMetrics);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching server metrics with details: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
        }

        // Fetch additional metrics (sensor, cpu core, disk partitions, network, etc.)
        foreach (var serverMetrics in serverMetricsList)
        {
            var sensorList = await FetchSensorDataAsync(serverId, serverMetrics.Time);
            serverMetrics.SensorList = sensorList;

            var cpuCoresList = await FetchCpuCoreDataAsync(serverId, serverMetrics.Time);
            serverMetrics.CpuCores = cpuCoresList;

            var diskPartitionsList = await FetchDiskPartitionDataAsync(serverId, serverMetrics.Time);
            serverMetrics.DiskPartitions = diskPartitionsList;

            var networkMetricsList = await FetchNetworkMetricsAsync(serverId, serverMetrics.Time);
            serverMetrics.NetworkMetrics = networkMetricsList;
        }

        return serverMetricsList;
    }

    private async Task<List<Sensor>> FetchSensorDataAsync(string serverId, DateTime time)
    {
        var sensorList = new List<Sensor>();

        var sensorQuery = @"
            SELECT sensor_name, value
            FROM sensor_data
            WHERE server_id = @serverId AND time = @time;
        ";

        await using var sensorCmd = new NpgsqlCommand(sensorQuery, conn);
        sensorCmd.Parameters.AddWithValue("serverId", serverId);
        sensorCmd.Parameters.AddWithValue("time", time);

        await using (var sensorReader = await sensorCmd.ExecuteReaderAsync())
        {
            try
            {
                while (await sensorReader.ReadAsync())
                {
                    sensorList.Add(new Sensor
                    {
                        Name = sensorReader.GetString(sensorReader.GetOrdinal("sensor_name")),
                        Value = sensorReader.GetInt32(sensorReader.GetOrdinal("value"))
                    });
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error fetching Sensor metrics with details: {ex.Message}");
            }
            finally
            {
                await sensorReader.CloseAsync();
            }
        }
        return sensorList;
    }

    private async Task<List<CoreMetrics>> FetchCpuCoreDataAsync(string serverId, DateTime time)
    {
        var cpuCoresList = new List<CoreMetrics>();

        var cpuCoresQuery = @"
            SELECT core_name, total, c_user, nice, system, idle, io_wait, irq, soft_irq, steal
            FROM core_metrics
            WHERE server_id = @serverId AND time = @time;
        ";

        await using var cpuCoresCmd = new NpgsqlCommand(cpuCoresQuery, conn);
        cpuCoresCmd.Parameters.AddWithValue("serverId", serverId);
        cpuCoresCmd.Parameters.AddWithValue("time", time);

        await using (var cpuCoresReader = await cpuCoresCmd.ExecuteReaderAsync())
        {
            try
            {
                while (await cpuCoresReader.ReadAsync())
                {
                    cpuCoresList.Add(new CoreMetrics
                    {
                        CoreName = cpuCoresReader.GetString(cpuCoresReader.GetOrdinal("core_name")),
                        Total = cpuCoresReader.GetDouble(cpuCoresReader.GetOrdinal("total")),
                        User = cpuCoresReader.GetDouble(cpuCoresReader.GetOrdinal("c_user")),
                        Nice = cpuCoresReader.GetDouble(cpuCoresReader.GetOrdinal("nice")),
                        System = cpuCoresReader.GetDouble(cpuCoresReader.GetOrdinal("system")),
                        Idle = cpuCoresReader.GetDouble(cpuCoresReader.GetOrdinal("idle")),
                        IOWait = cpuCoresReader.GetDouble(cpuCoresReader.GetOrdinal("io_wait")),
                        IRQ = cpuCoresReader.GetDouble(cpuCoresReader.GetOrdinal("irq")),
                        SoftIRQ = cpuCoresReader.GetDouble(cpuCoresReader.GetOrdinal("soft_irq")),
                        Steal = cpuCoresReader.GetDouble(cpuCoresReader.GetOrdinal("steal"))
                    });
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error fetching CoreMetrics metrics with details: {ex.Message}");
            }
            finally
            {
                await cpuCoresReader.CloseAsync();
            }
        }
        return cpuCoresList;
    }

    private async Task<List<DiskPartition>> FetchDiskPartitionDataAsync(string serverId, DateTime time)
    {
        var diskPartitionsList = new List<DiskPartition>();

        var diskPartitionsQuery = @"
            SELECT partition_name, read_speed, write_speed, io_time, weighted_io_time
            FROM disk_partitions
            WHERE server_id = @serverId AND time = @time;
        ";

        await using var diskPartitionsCmd = new NpgsqlCommand(diskPartitionsQuery, conn);
        diskPartitionsCmd.Parameters.AddWithValue("serverId", serverId);
        diskPartitionsCmd.Parameters.AddWithValue("time", time);

        await using (var diskPartitionsReader = await diskPartitionsCmd.ExecuteReaderAsync())
        {
            try
            {
                while (await diskPartitionsReader.ReadAsync())
                {
                    diskPartitionsList.Add(new DiskPartition
                    {
                        Name = diskPartitionsReader.GetString(diskPartitionsReader.GetOrdinal("partition_name")),
                        ReadSpeed = diskPartitionsReader.GetInt64(diskPartitionsReader.GetOrdinal("read_speed")),
                        WriteSpeed = diskPartitionsReader.GetInt64(diskPartitionsReader.GetOrdinal("write_speed")),
                        IoTime = diskPartitionsReader.GetInt64(diskPartitionsReader.GetOrdinal("io_time")),
                        WeightedIoTime = diskPartitionsReader.GetInt64(diskPartitionsReader.GetOrdinal("weighted_io_time"))
                    });
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error fetching DiskPartition metrics with details: {ex.Message}");
            }
            finally
            {
                await diskPartitionsReader.CloseAsync();
            }
        }
        return diskPartitionsList;
    }

    private async Task<List<NetworkMetric>> FetchNetworkMetricsAsync(string serverId, DateTime time)
    {
        var networkMetricsList = new List<NetworkMetric>();

        var networkMetricsQuery = @"
            SELECT interface_name, upload, download, receive_bytes, receive_packets, receive_errs, receive_drop, receive_fifo, receive_frame, receive_compressed, receive_multicast, 
            transmit_bytes, transmit_packets, transmit_errs, transmit_drop, transmit_fifo, transmit_colls, transmit_carrier, transmit_compressed
            FROM network_metrics
            WHERE server_id = @serverId AND time = @time;
        ";

        await using var networkMetricsCmd = new NpgsqlCommand(networkMetricsQuery, conn);
        networkMetricsCmd.Parameters.AddWithValue("serverId", serverId);
        networkMetricsCmd.Parameters.AddWithValue("time", time);

        await using (var networkMetricsReader = await networkMetricsCmd.ExecuteReaderAsync())
        {
            try
            {
                while (await networkMetricsReader.ReadAsync())
                {
                    NetworkInterfaceData iface = new NetworkInterfaceData
                    {
                        ReceiveBytes = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("receive_bytes")),
                        ReceivePackets = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("receive_packets")),
                        ReceiveErrs = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("receive_errs")),
                        ReceiveDrop = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("receive_drop")),
                        ReceiveFifo = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("receive_fifo")),
                        ReceiveFrame = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("receive_frame")),
                        ReceiveCompressed = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("receive_compressed")),
                        ReceiveMulticast = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("receive_multicast")),
                        TransmitBytes = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("transmit_bytes")),
                        TransmitPackets = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("transmit_packets")),
                        TransmitErrs = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("transmit_errs")),
                        TransmitDrop = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("transmit_drop")),
                        TransmitFifo = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("transmit_fifo")),
                        TransmitColls = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("transmit_colls")),
                        TransmitCarrier = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("transmit_carrier")),
                        TransmitCompressed = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("transmit_compressed"))
                    };

                    networkMetricsList.Add(new NetworkMetric
                    {
                        Name = networkMetricsReader.GetString(networkMetricsReader.GetOrdinal("interface_name")),
                        Upload = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("upload")),
                        Download = networkMetricsReader.GetInt64(networkMetricsReader.GetOrdinal("download")),
                        Iface = iface
                    });
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error fetching NetworkMetric metrics with details: {ex.Message}");
            }
            finally
            {
                await networkMetricsReader.CloseAsync();
            }
        }
        return networkMetricsList;
    }

    public async Task InsertServerMetricsAsync(DataPackage metrics)
    {
        if (conn.State != System.Data.ConnectionState.Open)
        {
            await conn.OpenAsync();
        }
        await ConvertToDataBaseObjects(metrics).InsertToDatabase(conn);
        await conn.CloseAsync();//this is reyatded
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
        
        serverMetrics.CpuCores = new List<CoreMetrics>();

        foreach(var dataPoint in data.Cpu.Cores)
        {
            var cpuCore = new CoreMetrics()
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

        serverMetrics.SensorList = new List<Sensor>();

        foreach(var dataPoint in data.SensorList.Sensors)
        {
            var sensor = new Sensor()
            {
                Name = dataPoint.Name,
                Value = dataPoint.Value
            };

            serverMetrics.SensorList.Add(sensor);
        }

        return serverMetrics;
    }
}