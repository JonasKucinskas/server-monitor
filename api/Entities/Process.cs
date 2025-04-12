using Npgsql;

public class Process
{
    public int id {get; set;}
    public int notification_id {get; set;}
    public int pid {get; set;}
    public string process_user {get; set;}
    public TimeOnly process_time {get; set;}
    public string system_ip {get; set;}
    public string name {get; set;}
    public float cpu_usage {get; set;}
    public float ram_usage {get; set;}
    public DateTime timestamp {get; set;}

    public async Task InsertToDatabase(NpgsqlConnection conn)
    {
        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO processes_data (pid, process_user, process_time, system_ip, name, cpu_usage, ram_usage, notification_id)
            VALUES (@pid, @process_user, @process_time, @system_ip, @name, @cpu_usage, @ram_usage, @notification_id);
        ", conn);

        cmd.Parameters.AddWithValue("pid", pid);
        cmd.Parameters.AddWithValue("process_user", process_user);
        cmd.Parameters.AddWithValue("process_time", process_time);
        cmd.Parameters.AddWithValue("system_ip", system_ip);
        cmd.Parameters.AddWithValue("name", name);
        cmd.Parameters.AddWithValue("cpu_usage", cpu_usage);
        cmd.Parameters.AddWithValue("ram_usage", ram_usage);
        cmd.Parameters.AddWithValue("notification_id", notification_id);

        await cmd.ExecuteNonQueryAsync();
    }
}