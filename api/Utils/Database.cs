using System.Data;
using Npgsql;

public class Database
{
    private static readonly Lazy<Database> _instance = new Lazy<Database>(() => new Database("Host=localhost;Port=5432;Username=postgres;Password=password;Database=monitor"));

    public static Database Instance => _instance.Value;

    private readonly NpgsqlConnection conn;
    private readonly string connectionString;
    public Database(string connectionString)
    {
        this.connectionString = connectionString;
        this.conn = new NpgsqlConnection(connectionString);
    }

    public async Task OpenConnAsync()
    {
        try
        {
            if (conn.State != System.Data.ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task CloseConnAsync()
    {
        if (conn.State == System.Data.ConnectionState.Open)
        {
            await conn.CloseAsync();
        }
    }



    public async Task<List<NetworkService>> FetchAllNetworkServices(string systemId)
    {
        await OpenConnAsync();

        var services = new List<NetworkService>();

        string networkServicesQuery;

        if (systemId == null)
        {
            networkServicesQuery = @"SELECT * FROM networkServices;";
        }
        else
        {
            networkServicesQuery = @"SELECT * FROM networkServices WHERE system_id = @systemId;";
        }

        await using var cmd = new NpgsqlCommand(networkServicesQuery, conn);

        if (systemId != null)
        {
            cmd.Parameters.AddWithValue("systemId", systemId);
        }

        await using var reader = await cmd.ExecuteReaderAsync();
        
        try
        {
            while (await reader.ReadAsync())
            {
                var networkService = new NetworkService
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    systemId = reader.GetString(reader.GetOrdinal("system_id")),
                    name = reader.GetString(reader.GetOrdinal("name")),
                    ip = reader.GetString(reader.GetOrdinal("ip")),
                    port = reader.GetInt32(reader.GetOrdinal("port")),
                    interval = reader.GetInt32(reader.GetOrdinal("interval")),
                    timeout = reader.GetInt32(reader.GetOrdinal("timeout")),
                    expected_status = reader.GetInt32(reader.GetOrdinal("expected_status")),
                    last_checked = reader.GetDateTime(reader.GetOrdinal("last_checked")),
                };

                services.Add(networkService);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error network services with details: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
            await CloseConnAsync();
        }

        return services;
    }

    public async Task<NetworkService> FetchNetworkServiceById(string systemId)
    {
        await OpenConnAsync();

        var service = new NetworkService();

        string serverMetricsQuery = @"
            SELECT * FROM networkServices WHERE system_id = @systemId;
        ";

        await using var cmd = new NpgsqlCommand(serverMetricsQuery, conn);
        cmd.Parameters.AddWithValue("systemId", systemId);

        await using var reader = await cmd.ExecuteReaderAsync();
        
        try
        {
            while (await reader.ReadAsync())
            {
                service.id = reader.GetInt32(reader.GetOrdinal("id"));
                service.systemId = reader.GetString(reader.GetOrdinal("system_id"));
                service.name = reader.GetString(reader.GetOrdinal("name"));
                service.ip = reader.GetString(reader.GetOrdinal("ip"));
                service.port = reader.GetInt32(reader.GetOrdinal("port"));
                service.interval = reader.GetInt32(reader.GetOrdinal("interval"));
                service.timeout = reader.GetInt32(reader.GetOrdinal("timeout"));
                service.expected_status = reader.GetInt32(reader.GetOrdinal("expected_status"));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error network services with details: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
            await CloseConnAsync();
        }

        return service;
    }
    
    public async Task<List<PingData>> FetchNetworkServicePings(int serviceId, DateTime startTime, DateTime endTime)
    {
        await OpenConnAsync();
        var pings = new List<PingData>();
        
        string networkServicePingsQuery = @"
            SELECT * FROM networkServicePings WHERE service_id = @serviceId AND timestamp BETWEEN @startTime AND @endTime;
        ";

        await using var cmd = new NpgsqlCommand(networkServicePingsQuery, conn);
        cmd.Parameters.AddWithValue("serviceId", serviceId);
        cmd.Parameters.AddWithValue("startTime", startTime);
        cmd.Parameters.AddWithValue("endTime", endTime);

        await using var reader = await cmd.ExecuteReaderAsync();
        
        try
        {
            while (await reader.ReadAsync())
            {
                var ping = new PingData
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")), 
                    serviceId = reader.GetInt32(reader.GetOrdinal("service_id")), 
                    isUp = reader.GetBoolean(reader.GetOrdinal("is_up")),  
                    responseTime = reader.GetFloat(reader.GetOrdinal("response_time")), 
                    errorMessage = reader.GetString(reader.GetOrdinal("error_message")), 
                    timestamp = reader.GetDateTime(reader.GetOrdinal("timestamp")),  
                };

                pings.Add(ping);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching pings with details: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
            await CloseConnAsync();
        }

        return pings;
    }

    public async Task<User> GetUser(string username, string password)
    {
        await OpenConnAsync();
        
        string query = @"
            SELECT * FROM users WHERE username = @username AND password_hash = @passwd_hash;
        ";

        await using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("username", username);
        cmd.Parameters.AddWithValue("passwd_hash", Hashing.HashPassword(password));

        await using var reader = await cmd.ExecuteReaderAsync();
        User user = null;

        try
        {
            if (await reader.ReadAsync())
            {
                user = new User
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    username = reader.GetString(reader.GetOrdinal("username")),
                    password = reader.GetString(reader.GetOrdinal("password_hash")),
                    creationDate = reader.GetDateTime(reader.GetOrdinal("created_at"))
                };

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding user: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
            await CloseConnAsync();
        }

        return user;
    }

    public async Task<User> GetUserById(int id)
    {
        await OpenConnAsync();
        
        string query = @"
            SELECT * FROM users WHERE id = @id;
        ";

        await using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        User user = null;

        try
        {
            if (await reader.ReadAsync())
            {
                user = new User
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    username = reader.GetString(reader.GetOrdinal("username")),
                    password = reader.GetString(reader.GetOrdinal("password_hash")),
                    creationDate = reader.GetDateTime(reader.GetOrdinal("created_at"))
                };

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding user: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
            await CloseConnAsync();
        }

        return user;
    }

    public async Task<bool> DoesUserExist(string username)
    {
        await OpenConnAsync();
        
        string query = @"
            SELECT * FROM users WHERE username = @username;
        ";

        await using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("username", username);

        await using var reader = await cmd.ExecuteReaderAsync();

        bool userExists = false;

        try
        {
            if (await reader.ReadAsync())
            {
                userExists = true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding user: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
            await CloseConnAsync();
        }

        return userExists;
    }

    public async Task InsertUser(User user)
    {
        await OpenConnAsync();
        await user.InsertToDatabase(conn);
        await CloseConnAsync();
    }

    public async Task<PingData> FetchLatestNetworkServicePing(int serviceId)
    {
        await OpenConnAsync();
        
        string networkServicePingsQuery = @"
            SELECT * FROM networkServicePings WHERE service_id = @serviceId ORDER BY timestamp DESC LIMIT 1;
        ";

        await using var cmd = new NpgsqlCommand(networkServicePingsQuery, conn);
        cmd.Parameters.AddWithValue("serviceId", serviceId);
        await using var reader = await cmd.ExecuteReaderAsync();
        
        var ping = new PingData();

        try
        {
            while (await reader.ReadAsync())
            {
                ping.id = reader.GetInt32(reader.GetOrdinal("id")); 
                ping.serviceId = reader.GetInt32(reader.GetOrdinal("service_id")); 
                ping.isUp = reader.GetBoolean(reader.GetOrdinal("is_up"));  
                ping.responseTime = reader.GetFloat(reader.GetOrdinal("response_time")); 
                ping.errorMessage = reader.GetString(reader.GetOrdinal("error_message")); 
                ping.timestamp = reader.GetDateTime(reader.GetOrdinal("timestamp"));  
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching ping with details: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
            await CloseConnAsync();
        }

        return ping;
    }

    public async Task<List<SystemData>> FetchAllSystemsByUserId(int userId)
    {
        await OpenConnAsync();
        var systems = new List<SystemData>();

        string serverMetricsQuery = @"
            SELECT * FROM servers WHERE owner_user_id = @userId;
        ";

        await using var cmd = new NpgsqlCommand(serverMetricsQuery, conn);
        cmd.Parameters.AddWithValue("userId", userId);

        await using var reader = await cmd.ExecuteReaderAsync();
        
        try
        {
            while (await reader.ReadAsync())
            {
                var systemData = new SystemData
                {
                    id = reader.GetInt32(reader.GetOrdinal("server_id")),
                    ip = reader.GetString(reader.GetOrdinal("server_ip")),
                    port = reader.GetInt32(reader.GetOrdinal("server_port")),
                    name = reader.GetString(reader.GetOrdinal("system_name")),
                    ownerId = reader.GetInt32(reader.GetOrdinal("owner_user_id")),
                    updateInterval = reader.GetInt32(reader.GetOrdinal("interval")),
                    creationDate = reader.GetDateTime(reader.GetOrdinal("date_deployed")),
                };

                systems.Add(systemData);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching server metrics with details: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
            await CloseConnAsync();
        }

        return systems;
    }

    public async Task<User> FetchSystemOwnerAsync(SystemData system)
    {
        await OpenConnAsync();

        string serverMetricsQuery = @"
            SELECT * FROM users WHERE id = @userid;
        ";

        await using var cmd = new NpgsqlCommand(serverMetricsQuery, conn);
        cmd.Parameters.AddWithValue("userid", system.ownerId);

        await using var reader = await cmd.ExecuteReaderAsync();

        User user = null;

        try
        {
            if (await reader.ReadAsync())
            {
                user = new User
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    username = reader.GetString(reader.GetOrdinal("username")),
                    password = reader.GetString(reader.GetOrdinal("password_hash")),
                    creationDate = reader.GetDateTime(reader.GetOrdinal("created_at"))
                };

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding user: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
            await CloseConnAsync();
        }

        return user;
    }

    public async Task<List<SystemData>> FetchAllSystems()
    {
        await OpenConnAsync();
        var systems = new List<SystemData>();

        string serverMetricsQuery = @"
            SELECT * FROM servers;
        ";

        await using var cmd = new NpgsqlCommand(serverMetricsQuery, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        
        try
        {
            while (await reader.ReadAsync())
            {
                var systemData = new SystemData
                {
                    id = reader.GetInt32(reader.GetOrdinal("server_id")),
                    ip = reader.GetString(reader.GetOrdinal("server_ip")),
                    port = reader.GetInt32(reader.GetOrdinal("server_port")),
                    name = reader.GetString(reader.GetOrdinal("system_name")),
                    ownerId = reader.GetInt32(reader.GetOrdinal("owner_user_id")),
                    updateInterval = reader.GetInt32(reader.GetOrdinal("interval")),
                    creationDate = reader.GetDateTime(reader.GetOrdinal("date_deployed")),
                };

                systems.Add(systemData);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching server metrics with details: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
            await CloseConnAsync();
        }

        return systems;
    }


    public async Task<SystemData> FetchSystemByName(string systemName)
    {
        await OpenConnAsync();
        string serverMetricsQuery = @"
            SELECT * FROM servers WHERE system_name = @systemName;
        ";

        await using var cmd = new NpgsqlCommand(serverMetricsQuery, conn);
        cmd.Parameters.AddWithValue("systemName", systemName);

        await using var reader = await cmd.ExecuteReaderAsync();
        
        var systemData = new SystemData();

        try
        {
            while (await reader.ReadAsync())
            {
                systemData.id = reader.GetInt32(reader.GetOrdinal("server_id"));
                systemData.ip = reader.GetString(reader.GetOrdinal("server_ip"));
                systemData.port = reader.GetInt32(reader.GetOrdinal("server_port"));
                systemData.name = reader.GetString(reader.GetOrdinal("system_name"));
                systemData.ownerId = reader.GetInt32(reader.GetOrdinal("owner_user_id"));
                systemData.updateInterval = reader.GetInt32(reader.GetOrdinal("interval"));
                systemData.creationDate = reader.GetDateTime(reader.GetOrdinal("date_deployed"));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching system by name: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
            await CloseConnAsync();
        }

        return systemData;
    }

    public async Task<SystemData> FetchSystemByIpAsync(string ip)
    {
        await OpenConnAsync();
        string serverMetricsQuery = @"
            SELECT * FROM servers WHERE server_ip = @ip;
        ";

        await using var cmd = new NpgsqlCommand(serverMetricsQuery, conn);
        cmd.Parameters.AddWithValue("ip", ip);

        await using var reader = await cmd.ExecuteReaderAsync();
        
        var systemData = new SystemData();

        try
        {
            while (await reader.ReadAsync())
            {
                systemData.id = reader.GetInt32(reader.GetOrdinal("server_id"));
                systemData.ip = reader.GetString(reader.GetOrdinal("server_ip"));
                systemData.port = reader.GetInt32(reader.GetOrdinal("server_port"));
                systemData.name = reader.GetString(reader.GetOrdinal("system_name"));
                systemData.ownerId = reader.GetInt32(reader.GetOrdinal("owner_user_id"));
                systemData.updateInterval = reader.GetInt32(reader.GetOrdinal("interval"));
                systemData.creationDate = reader.GetDateTime(reader.GetOrdinal("date_deployed"));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching system by ip: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
            await CloseConnAsync();
        }

        return systemData;
    }

    public async Task<List<ServerMetrics>> FetchServerMetrics(string systemName, DateTime startTime, DateTime endTime)
    {
        await OpenConnAsync();

        var serverMetricsList = new List<ServerMetrics>();
        string serverIp = "";

        string getIpQuery = @"
            SELECT server_ip FROM servers WHERE system_name = @systemName;
        ";
        await using var cmd = new NpgsqlCommand(getIpQuery, conn);
        cmd.Parameters.AddWithValue("systemName", systemName);


        await using var reader = await cmd.ExecuteReaderAsync();
        try
        {
            while (await reader.ReadAsync())
            {
                serverIp = reader.GetString(reader.GetOrdinal("server_ip"));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching server ip: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
        }

        if (serverIp == "")
        {
            Console.WriteLine("faiuled to get serveriip");
            return serverMetricsList;
        }

        string serverMetricsQuery = @"
            SELECT time, server_id, cpu_name, cpu_freq, battery_name, battery_capacity, battery_status,
                disk_total_space, disk_used_space, disk_free_space, ram_mem_total, ram_mem_free, ram_mem_used,
                ram_mem_available, ram_buffers, ram_cached, ram_swap_total, ram_swap_free, ram_swap_used
            FROM server_metrics
            WHERE server_id = @serverId
            AND time BETWEEN @startTime AND @endTime
            ORDER BY time;
        ";

        await using var cmd2 = new NpgsqlCommand(serverMetricsQuery, conn);
        cmd2.Parameters.AddWithValue("serverId", serverIp);
        cmd2.Parameters.AddWithValue("startTime", startTime);
        cmd2.Parameters.AddWithValue("endTime", endTime);

        await using var reader2 = await cmd2.ExecuteReaderAsync();

        try
        {
            while (await reader2.ReadAsync())
            {
                var serverMetrics = new ServerMetrics
                {
                    Time = reader2.GetDateTime(reader2.GetOrdinal("time")),
                    ServerId = reader2.GetString(reader2.GetOrdinal("server_id")),
                    CpuName = reader2.GetString(reader2.GetOrdinal("cpu_name")),
                    CpuFreq = reader2.GetDouble(reader2.GetOrdinal("cpu_freq")),
                    BatteryName = reader2.GetString(reader2.GetOrdinal("battery_name")),
                    BatteryCapacity = reader2.GetInt32(reader2.GetOrdinal("battery_capacity")),
                    BatteryStatus = reader2.GetString(reader2.GetOrdinal("battery_status")),
                    DiskTotalSpace = reader2.GetInt64(reader2.GetOrdinal("disk_total_space")),
                    DiskUsedSpace = reader2.GetInt64(reader2.GetOrdinal("disk_used_space")),
                    DiskFreeSpace = reader2.GetInt64(reader2.GetOrdinal("disk_free_space")),
                    RamMemTotal = reader2.GetInt64(reader2.GetOrdinal("ram_mem_total")),
                    RamMemFree = reader2.GetInt64(reader2.GetOrdinal("ram_mem_free")),
                    RamMemUsed = reader2.GetInt64(reader2.GetOrdinal("ram_mem_used")),
                    RamMemAvailable = reader2.GetInt64(reader2.GetOrdinal("ram_mem_available")),
                    RamBuffers = reader2.GetInt64(reader2.GetOrdinal("ram_buffers")),
                    RamCached = reader2.GetInt64(reader2.GetOrdinal("ram_cached")),
                    RamSwapTotal = reader2.GetInt64(reader2.GetOrdinal("ram_swap_total")),
                    RamSwapFree = reader2.GetInt64(reader2.GetOrdinal("ram_swap_free")),
                    RamSwapUsed = reader2.GetInt64(reader2.GetOrdinal("ram_swap_used"))
                };

                serverMetricsList.Add(serverMetrics);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching server metrics with details: {ex.Message}");
        }
        finally
        {
            await reader2.CloseAsync();
            await CloseConnAsync();
        }

        foreach (var serverMetrics in serverMetricsList)
        {
            var sensorList = await FetchSensorDataAsync(serverIp, serverMetrics.Time);
            serverMetrics.SensorList = sensorList;

            var cpuCoresList = await FetchCpuCoreDataAsync(serverIp, serverMetrics.Time);
            serverMetrics.CpuCores = cpuCoresList;

            var diskPartitionsList = await FetchDiskPartitionDataAsync(serverIp, serverMetrics.Time);
            serverMetrics.DiskPartitions = diskPartitionsList;

            var networkMetricsList = await FetchNetworkMetricsAsync(serverIp, serverMetrics.Time);
            serverMetrics.NetworkMetrics = networkMetricsList;
        }

        return serverMetricsList;
    }

    public async Task<ServerMetrics> FetchLatestMetrics(string systemName)
    {
        await OpenConnAsync();

        string serverIp = "";

        string getIpQuery = @"
            SELECT server_ip FROM servers WHERE system_name = @systemName;
        ";
        await using var cmd = new NpgsqlCommand(getIpQuery, conn);
        cmd.Parameters.AddWithValue("systemName", systemName);


        await using var reader = await cmd.ExecuteReaderAsync();
        try
        {
            while (await reader.ReadAsync())
            {
                serverIp = reader.GetString(reader.GetOrdinal("server_ip"));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching server ip: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
        }

        if (serverIp == "")
        {
            Console.WriteLine("faiuled to get serveriip");
            return null;
        }

        string latestServerMetricsQuery = @"
            SELECT time, server_id, cpu_name, cpu_freq, battery_name, battery_capacity, battery_status,
                disk_total_space, disk_used_space, disk_free_space, ram_mem_total, ram_mem_free, ram_mem_used,
                ram_mem_available, ram_buffers, ram_cached, ram_swap_total, ram_swap_free, ram_swap_used
            FROM server_metrics
            WHERE server_id = @serverId
            ORDER BY time DESC
            LIMIT 1;
        ";


        await using var cmd2 = new NpgsqlCommand(latestServerMetricsQuery, conn);
        cmd2.Parameters.AddWithValue("serverId", serverIp);

        await using var reader2 = await cmd2.ExecuteReaderAsync();

        var serverMetrics = new ServerMetrics();

        try
        {
            while (await reader2.ReadAsync())
            {
                serverMetrics.Time = reader2.GetDateTime(reader2.GetOrdinal("time"));
                serverMetrics.ServerId = reader2.GetString(reader2.GetOrdinal("server_id"));
                serverMetrics.CpuName = reader2.GetString(reader2.GetOrdinal("cpu_name"));
                serverMetrics.CpuFreq = reader2.GetDouble(reader2.GetOrdinal("cpu_freq"));
                serverMetrics.BatteryName = reader2.GetString(reader2.GetOrdinal("battery_name"));
                serverMetrics.BatteryCapacity = reader2.GetInt32(reader2.GetOrdinal("battery_capacity"));
                serverMetrics.BatteryStatus = reader2.GetString(reader2.GetOrdinal("battery_status"));
                serverMetrics.DiskTotalSpace = reader2.GetInt64(reader2.GetOrdinal("disk_total_space"));
                serverMetrics.DiskUsedSpace = reader2.GetInt64(reader2.GetOrdinal("disk_used_space"));
                serverMetrics.DiskFreeSpace = reader2.GetInt64(reader2.GetOrdinal("disk_free_space"));
                serverMetrics.RamMemTotal = reader2.GetInt64(reader2.GetOrdinal("ram_mem_total"));
                serverMetrics.RamMemFree = reader2.GetInt64(reader2.GetOrdinal("ram_mem_free"));
                serverMetrics.RamMemUsed = reader2.GetInt64(reader2.GetOrdinal("ram_mem_used"));
                serverMetrics.RamMemAvailable = reader2.GetInt64(reader2.GetOrdinal("ram_mem_available"));
                serverMetrics.RamBuffers = reader2.GetInt64(reader2.GetOrdinal("ram_buffers"));
                serverMetrics.RamCached = reader2.GetInt64(reader2.GetOrdinal("ram_cached"));
                serverMetrics.RamSwapTotal = reader2.GetInt64(reader2.GetOrdinal("ram_swap_total"));
                serverMetrics.RamSwapFree = reader2.GetInt64(reader2.GetOrdinal("ram_swap_free"));
                serverMetrics.RamSwapUsed = reader2.GetInt64(reader2.GetOrdinal("ram_swap_used"));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching server metrics with details: {ex.Message}");
        }
        finally
        {
            await reader2.CloseAsync();
            await CloseConnAsync();
        }

        var sensorList = await FetchSensorDataAsync(serverIp, serverMetrics.Time);
        serverMetrics.SensorList = sensorList;
        var cpuCoresList = await FetchCpuCoreDataAsync(serverIp, serverMetrics.Time);
        serverMetrics.CpuCores = cpuCoresList;
        var diskPartitionsList = await FetchDiskPartitionDataAsync(serverIp, serverMetrics.Time);
        serverMetrics.DiskPartitions = diskPartitionsList;
        var networkMetricsList = await FetchNetworkMetricsAsync(serverIp, serverMetrics.Time);
        serverMetrics.NetworkMetrics = networkMetricsList;
        
        return serverMetrics;
    }

    private async Task<List<Sensor>> FetchSensorDataAsync(string serverIp, DateTime time)
    {
        await OpenConnAsync();
        var sensorList = new List<Sensor>();

        var sensorQuery = @"
            SELECT sensor_name, value
            FROM sensor_data
            WHERE server_id = @serverId AND time = @time;
        ";

        await using var sensorCmd = new NpgsqlCommand(sensorQuery, conn);
        sensorCmd.Parameters.AddWithValue("serverId", serverIp);
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
                await CloseConnAsync();
            }
        }
        return sensorList;
    }

    private async Task<List<CoreMetrics>> FetchCpuCoreDataAsync(string serverIp, DateTime time)
    {
        await OpenConnAsync();
        var cpuCoresList = new List<CoreMetrics>();

        var cpuCoresQuery = @"
            SELECT core_name, total, c_user, nice, system, idle, io_wait, irq, soft_irq, steal
            FROM core_metrics
            WHERE server_id = @serverId AND time = @time;
        ";

        await using var cpuCoresCmd = new NpgsqlCommand(cpuCoresQuery, conn);
        cpuCoresCmd.Parameters.AddWithValue("serverId", serverIp);
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
                await CloseConnAsync();
            }
        }
        return cpuCoresList;
    }

    private async Task<List<DiskPartition>> FetchDiskPartitionDataAsync(string serverIp, DateTime time)
    {
        await OpenConnAsync();
        var diskPartitionsList = new List<DiskPartition>();

        var diskPartitionsQuery = @"
            SELECT partition_name, read_speed, write_speed, io_time, weighted_io_time
            FROM disk_partitions
            WHERE server_id = @serverId AND time = @time;
        ";

        await using var diskPartitionsCmd = new NpgsqlCommand(diskPartitionsQuery, conn);
        diskPartitionsCmd.Parameters.AddWithValue("serverId", serverIp);
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
                await CloseConnAsync();
            }
        }
        return diskPartitionsList;
    }

    private async Task<List<NetworkMetric>> FetchNetworkMetricsAsync(string serverIp, DateTime time)
    {
        await OpenConnAsync();
        var networkMetricsList = new List<NetworkMetric>();

        var networkMetricsQuery = @"
            SELECT interface_name, upload, download, receive_bytes, receive_packets, receive_errs, receive_drop, receive_fifo, receive_frame, receive_compressed, receive_multicast, 
            transmit_bytes, transmit_packets, transmit_errs, transmit_drop, transmit_fifo, transmit_colls, transmit_carrier, transmit_compressed
            FROM network_metrics
            WHERE server_id = @serverId AND time = @time;
        ";

        await using var networkMetricsCmd = new NpgsqlCommand(networkMetricsQuery, conn);
        networkMetricsCmd.Parameters.AddWithValue("serverId", serverIp);
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
                await CloseConnAsync();
            }
        }
        return networkMetricsList;
    }

    public async Task<List<NotificationRule>> FetchNotificationRulesAsync(int userId, string ip)
    {
        await OpenConnAsync();
        var notificationRulesList = new List<NotificationRule>();

        var notificationRulesCmdQuery = @"
        
            SELECT id, user_id, system_ip, resource, usage, timestamp
            FROM notification_rules
            WHERE system_ip = @ip AND user_id = @userid;
        ";

        await using var notificationRulesCmd = new NpgsqlCommand(notificationRulesCmdQuery, conn);
        notificationRulesCmd.Parameters.AddWithValue("userid", userId);
        notificationRulesCmd.Parameters.AddWithValue("ip", ip);

        await using (var notificationRulesReader = await notificationRulesCmd.ExecuteReaderAsync())
        {
            try
            {
                while (await notificationRulesReader.ReadAsync())
                {

                    NotificationRule rule = new NotificationRule
                    {
                        id = notificationRulesReader.GetInt32(notificationRulesReader.GetOrdinal("id")),
                        userId = notificationRulesReader.GetInt32(notificationRulesReader.GetOrdinal("user_id")),
                        systemIp = notificationRulesReader.GetString(notificationRulesReader.GetOrdinal("system_ip")),            
                        resource = notificationRulesReader.GetString(notificationRulesReader.GetOrdinal("resource")),
                        usage = notificationRulesReader.GetFloat(notificationRulesReader.GetOrdinal("usage")),
                        timestamp = notificationRulesReader.GetDateTime(notificationRulesReader.GetOrdinal("timestamp")),
                    };

                    notificationRulesList.Add(rule);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error fetching notification rules with details: {ex.Message}");
            }
            finally
            {
                await notificationRulesReader.CloseAsync();
                await CloseConnAsync();
            }
        }
        return notificationRulesList;
    }

    public async Task InsertSystemAsync(SystemData system)
    {
        await OpenConnAsync();
        await system.InsertToDatabase(conn);
        await CloseConnAsync();
        await SshConnection.Instance.StartSendingRequests(system.ip, system.port, "monitor", system.updateInterval);
    }

    public async Task<NotificationRule> InsertNotificationRuleAsync(NotificationRule rule)
    {
        await OpenConnAsync();
        await rule.InsertToDatabase(conn);
        await CloseConnAsync();
        return rule;
    }

    public async Task DeleteSystemAsync(int id, int userid)
    {
        await OpenConnAsync();

        await using var cmd = new NpgsqlCommand(@"DELETE FROM servers WHERE server_id = @Id AND owner_user_id = @userid;", conn);

        cmd.Parameters.AddWithValue("Id", id);
        cmd.Parameters.AddWithValue("userid", userid);

        await cmd.ExecuteNonQueryAsync();
        
        await CloseConnAsync();
    }

    public async Task UpdateUserPasswordAsync(User user)
    {
        await OpenConnAsync();
    
        await using var cmd = new NpgsqlCommand(@"
            UPDATE users
            SET password_hash = @newPassword
            WHERE id = @userId;", conn);

        cmd.Parameters.AddWithValue("newPassword", user.password);
        cmd.Parameters.AddWithValue("userId", user.id);

        await cmd.ExecuteNonQueryAsync();
        
        await CloseConnAsync();
    }


    public async Task DeleteNotificationsByRuleIdAsync(int ruleid)
    {
        await OpenConnAsync();

        await using var cmd = new NpgsqlCommand(@"DELETE FROM notifications WHERE notification_rule_id = @ruleid;", conn);

        cmd.Parameters.AddWithValue("ruleid", ruleid);

        await cmd.ExecuteNonQueryAsync();
        
        await CloseConnAsync();
    }

    public async Task DeleteNotificationRuleByIdAsync(int ruleid)
    {
        await OpenConnAsync();

        await using var cmd = new NpgsqlCommand(@"DELETE FROM notification_rules WHERE id = @ruleid;", conn);

        cmd.Parameters.AddWithValue("ruleid", ruleid);

        await cmd.ExecuteNonQueryAsync();
        
        await CloseConnAsync();
    }

    public async Task DeleteNotificationByIdAsync(int notificationid)
    {
        await OpenConnAsync();

        await using var cmd = new NpgsqlCommand(@"DELETE FROM notifications WHERE id = @id;", conn);

        cmd.Parameters.AddWithValue("id", notificationid);

        await cmd.ExecuteNonQueryAsync();
        
        await CloseConnAsync();
    }

    public async Task UpdateSystem(SystemData system, int userid)
    {
        await OpenConnAsync();
        
        await using var cmd = new NpgsqlCommand(@"UPDATE servers 
        SET system_name = @name, server_ip = @ip, server_port = @port, interval = @interval WHERE server_id = @id AND owner_user_id = @owner_id;", conn);

        cmd.Parameters.AddWithValue("name", system.name);
        cmd.Parameters.AddWithValue("ip", system.ip);
        cmd.Parameters.AddWithValue("port", system.port);
        cmd.Parameters.AddWithValue("interval", system.updateInterval);
        cmd.Parameters.AddWithValue("id", system.id);
        cmd.Parameters.AddWithValue("owner_id", userid);
        
        await cmd.ExecuteNonQueryAsync();
        await CloseConnAsync();
    }

    public async Task InsertServerMetricsAsync(DataPackage metrics, string ip)
    {
        await OpenConnAsync();
        await ConvertToDataBaseObjects(metrics, ip).InsertToDatabase(conn);
        await CloseConnAsync();
    }

    public async Task InsertNetworkServiceAsync(NetworkService service)
    {
        await OpenConnAsync();
        await service.InsertToDatabase(conn);
        await CloseConnAsync();
    }

    public async Task<Notification> InsertNotificationAsync(Notification notification)
    {
        await OpenConnAsync();
        await notification.InsertToDatabase(conn);
        await CloseConnAsync();
        return notification;
    }

    public async Task UpdateNetworkService(NetworkService service)
    {
        await OpenConnAsync();
        
        await using var cmd = new NpgsqlCommand(@"UPDATE networkServices SET name = @name, ip = @ip, port = @port, interval = @interval, timeout = @timeout, expected_status = @expected_status WHERE id = @id;", conn);

        cmd.Parameters.AddWithValue("name", service.name);
        cmd.Parameters.AddWithValue("ip", service.ip);
        cmd.Parameters.AddWithValue("port", service.port);
        cmd.Parameters.AddWithValue("interval", service.interval);
        cmd.Parameters.AddWithValue("timeout", service.timeout);
        cmd.Parameters.AddWithValue("expected_status", service.expected_status);
        cmd.Parameters.AddWithValue("id", service.id);
        
        await cmd.ExecuteNonQueryAsync();
        await CloseConnAsync();
    }

    public async Task DeleteNetworkService(int serviceId)
    {
        await OpenConnAsync();

        await using var cmd = new NpgsqlCommand(@"DELETE FROM networkServices WHERE id = @serviceId;", conn);

        cmd.Parameters.AddWithValue("serviceId", serviceId);

        await cmd.ExecuteNonQueryAsync();
        
        await CloseConnAsync();
        await DeleteNetworkServicePings(serviceId);
    }

    public async Task DeleteNetworkServicePings(int serviceId)
    {
        await OpenConnAsync();

        await using var cmd = new NpgsqlCommand(@"DELETE FROM networkServicePings WHERE service_id = @serviceId;", conn);

        cmd.Parameters.AddWithValue("serviceId", serviceId);

        await cmd.ExecuteNonQueryAsync();
        
        await CloseConnAsync();
    }

    public async Task InsertNetworkServicePing(PingData ping)
    {
        using (var conn2 = new NpgsqlConnection(connectionString)) 
        {

            await conn2.OpenAsync();
            await ping.InsertToDatabase(conn2);
            await conn2.CloseAsync();
        }
    }
    

    private static ServerMetrics ConvertToDataBaseObjects(DataPackage data, string ip)
    {
        ServerMetrics serverMetrics = new ServerMetrics()
        {
            Time = DateTime.Now,
            ServerId = ip,
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

    public async Task InsertProcess(Process process)
    {
        await OpenConnAsync();
        await process.InsertToDatabase(conn);
        await CloseConnAsync();
    }

    public async Task<List<NotificationRule>> FetchAllNotificationRulesAsync(string systemIp, int userId)
    {
        await OpenConnAsync();

        var rules = new List<NotificationRule>();

        string notitificationRulesQuery = @"SELECT * FROM notification_rules WHERE system_ip = @systemIp AND user_id = @userId";

        await using var cmd = new NpgsqlCommand(notitificationRulesQuery, conn);
        cmd.Parameters.AddWithValue("systemIp", systemIp);
        cmd.Parameters.AddWithValue("userId", userId);


        await using var reader = await cmd.ExecuteReaderAsync();
        
        try
        {
            while (await reader.ReadAsync())
            {
                var rule = new NotificationRule
                {

                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    userId = reader.GetInt32(reader.GetOrdinal("user_id")),
                    systemIp = reader.GetString(reader.GetOrdinal("system_ip")),         
                    resource = reader.GetString(reader.GetOrdinal("resource")),          
                    usage = reader.GetFloat(reader.GetOrdinal("usage")),
                    timestamp = reader.GetDateTime(reader.GetOrdinal("timestamp"))
                };

                rules.Add(rule);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching notifiction rules: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
            await CloseConnAsync();
        }

        return rules;
    }

    public async Task<List<Process>> FetchProcessesByNotificationIdAsync(int notificationId)
    {
        await OpenConnAsync();

        var processes = new List<Process>();

        string notificationRulesQuery = @"SELECT * FROM processes_data WHERE notification_id = @notificationId";

        await using var cmd = new NpgsqlCommand(notificationRulesQuery, conn);
        cmd.Parameters.AddWithValue("notificationId", notificationId);

        await using var reader = await cmd.ExecuteReaderAsync();
        
        try
        {
            while (await reader.ReadAsync())
            {
                var process = new Process
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    notification_id = reader.GetInt32(reader.GetOrdinal("notification_id")),
                    pid = reader.GetInt32(reader.GetOrdinal("pid")),
                    process_user = reader.GetString(reader.GetOrdinal("process_user")),
                    process_time = TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("process_time"))),
                    system_ip = reader.GetString(reader.GetOrdinal("system_ip")),
                    name = reader.GetString(reader.GetOrdinal("name")),
                    cpu_usage = reader.GetFloat(reader.GetOrdinal("cpu_usage")),
                    ram_usage = reader.GetFloat(reader.GetOrdinal("ram_usage")),
                    timestamp = reader.GetDateTime(reader.GetOrdinal("timestamp")),
                };

                processes.Add(process);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching processes: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
            await CloseConnAsync();
        }

        return processes;
    }

    public async Task<List<Notification>> FetchNotificationsByRuleIdAsync(int ruleId, int count)
    {
        await OpenConnAsync();

        var notifications = new List<Notification>();

        string notificatiosQuery = @"SELECT * FROM notifications WHERE notification_rule_id = @ruleId ORDER BY timestamp DESC";


        if (count == 1)
        {
            notificatiosQuery = @"SELECT * FROM notifications WHERE notification_rule_id = @ruleId ORDER BY timestamp DESC LIMIT 1";
        }


        await using var cmd = new NpgsqlCommand(notificatiosQuery, conn);
        cmd.Parameters.AddWithValue("ruleId", ruleId);

        await using var reader = await cmd.ExecuteReaderAsync();
        
        try
        {
            while (await reader.ReadAsync())
            {
                var notification = new Notification
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),          
                    notification_rule_id = reader.GetInt32(reader.GetOrdinal("notification_rule_id")), 
                    resource = reader.GetString(reader.GetOrdinal("resource")),            
                    usage = reader.GetFloat(reader.GetOrdinal("usage")),        
                    timestamp = reader.GetDateTime(reader.GetOrdinal("timestamp")),   
                };

                notifications.Add(notification);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching notifications: {ex.Message}");
        }
        finally
        {
            await reader.CloseAsync();
            await CloseConnAsync();
        }

        return notifications;
    }
}