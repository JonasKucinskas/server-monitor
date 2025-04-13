using Npgsql;

public class NotificationRule
{
    public int id { get; set; }         
    public int userId { get; set; }
    public string systemIp { get; set; }             
    public string resource { get; set; }           
    public float usage { get; set; }       
    public DateTime timestamp { get; set; }                 


    public async Task InsertToDatabase(NpgsqlConnection conn)
    {
        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO notification_rules (user_id, system_ip, resource, usage)
            VALUES (@user_id, @system_ip, @resource, @usage)
            RETURNING id, timestamp;
        ", conn);

        cmd.Parameters.AddWithValue("user_id", userId);
        cmd.Parameters.AddWithValue("system_ip", systemIp);
        cmd.Parameters.AddWithValue("resource", resource);
        cmd.Parameters.AddWithValue("usage", usage);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            id = reader.GetInt32(0);
            timestamp = reader.GetDateTime(1);
        }
    }
}
