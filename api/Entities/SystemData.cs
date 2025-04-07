using Npgsql;

public class SystemData
{
    public int id { get; set; }
    public string ip { get; set; }
    public int port { get; set; }
    public string name { get; set; }
    public int ownerId { get; set; }
    public DateTime creationDate { get; set; }


    public async Task InsertToDatabase(NpgsqlConnection conn)
    {
        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO servers (server_ip, server_port, system_name, owner_user_id)
            VALUES (@ip, @port, @name, @owner_id);
        ", conn);

        cmd.Parameters.AddWithValue("ip", ip);
        cmd.Parameters.AddWithValue("port", port);
        cmd.Parameters.AddWithValue("name", name);
        cmd.Parameters.AddWithValue("owner_id", ownerId);

        await cmd.ExecuteNonQueryAsync();
    }
}