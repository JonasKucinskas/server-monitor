using Npgsql;

public class PingData
{
    public int id { get; set; }
    public int serviceId { get; set; }
    public bool isUp { get; set; } 
    public float responseTime { get; set; }
    public string errorMessage { get; set; }
    public DateTime timestamp { get; set; }    

    public async Task InsertToDatabase(NpgsqlConnection conn)
    {
        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO networkServicePings (service_id, is_up, response_time, error_message)
            VALUES (@service_id, @is_up, @response_time, @error_message);
        ", conn);

        cmd.Parameters.AddWithValue("service_id", serviceId);
        cmd.Parameters.AddWithValue("is_up", isUp);
        cmd.Parameters.AddWithValue("response_time", responseTime);
        cmd.Parameters.AddWithValue("error_message", errorMessage);

        await cmd.ExecuteNonQueryAsync();
    }
}