using Npgsql;

public class Notification
{
    public int id { get; set; }         
    public int notification_rule_id { get; set; }
    public string resource { get; set; }           
    public float usage { get; set; }       
    public DateTime timestamp { get; set; }            

    public async Task InsertToDatabase(NpgsqlConnection conn)
    {
        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO notifications (notification_rule_id, resource, usage)
            VALUES (@notification_rule_id, @resource, @usage)
            RETURNING id, timestamp;
        ", conn);

        cmd.Parameters.AddWithValue("notification_rule_id", notification_rule_id);
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