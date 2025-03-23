using Npgsql;
using System.Text.Json.Serialization;

public class NetworkService
{
    public int id { get; set; }
    public string systemId { get; set; }
    public string name { get; set; }
    public string ip { get; set; }
    public int port { get; set; }
    public int interval { get; set; }
    public int timeout { get; set; }
    public int expected_status { get; set; }
    public DateTime last_checked { get; set; } 

    public async Task InsertToDatabase(NpgsqlConnection conn)
    {
        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO networkServices (system_id, name, ip, port, interval, timeout, expected_status)
            VALUES (@system_id, @name, @ip, @port, @interval, @timeout, @expected_status);
        ", conn);

        cmd.Parameters.AddWithValue("system_id", systemId);
        cmd.Parameters.AddWithValue("name", name);
        cmd.Parameters.AddWithValue("ip", ip);
        cmd.Parameters.AddWithValue("port", port);
        cmd.Parameters.AddWithValue("interval", interval);
        cmd.Parameters.AddWithValue("timeout", timeout);
        cmd.Parameters.AddWithValue("expected_status", expected_status);

        await cmd.ExecuteNonQueryAsync();
    }
}


