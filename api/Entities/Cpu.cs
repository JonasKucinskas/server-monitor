using Npgsql;

public class Cpu
{
    public string Name { get; set; }
    public List<CoreMetrics> Cores { get; set; }
    public double Freq { get; set; }
}

public class CoreMetrics
{
    public string CoreName { get; set; }
    public double Total { get; set; }
    public double User { get; set; }
    public double Nice { get; set; }
    public double System { get; set; }
    public double Idle { get; set; }
    public double IOWait { get; set; }
    public double IRQ { get; set; }
    public double SoftIRQ { get; set; }
    public double Steal { get; set; }

    public async Task InsertToDatabase(NpgsqlConnection conn, DateTime time, string serverId)
    {
        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO core_metrics (time, server_id, core_name, total, c_user, nice, system, idle, io_wait, irq, soft_irq, steal)
            VALUES (@time, @serverId, @coreName, @total, @user, @nice, @system, @idle, @iowait, @irq, @softirq, @steal);
        ", conn);

        cmd.Parameters.AddWithValue("time", time);
        cmd.Parameters.AddWithValue("serverId", serverId);
        cmd.Parameters.AddWithValue("coreName", CoreName);
        cmd.Parameters.AddWithValue("total", Total);
        cmd.Parameters.AddWithValue("user", User);
        cmd.Parameters.AddWithValue("nice", Nice);
        cmd.Parameters.AddWithValue("system", System);
        cmd.Parameters.AddWithValue("idle", Idle);
        cmd.Parameters.AddWithValue("iowait", IOWait);
        cmd.Parameters.AddWithValue("irq", IRQ);
        cmd.Parameters.AddWithValue("softirq", SoftIRQ);
        cmd.Parameters.AddWithValue("steal", Steal);
        
        await cmd.ExecuteNonQueryAsync();
    }
}