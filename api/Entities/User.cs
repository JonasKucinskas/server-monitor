using Npgsql;
using Renci.SshNet;

public class User
{
    public int id { get; set; }
    public string username { get; set; }
    public string password { get; set; }
    public DateTime creationDate { get; set; }


    public async Task InsertToDatabase(NpgsqlConnection conn)
    {
        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO users (username, password_hash)
            VALUES (@username, @passwordHash);
        ", conn);

        cmd.Parameters.AddWithValue("username", username);
        cmd.Parameters.AddWithValue("passwordHash", Hashing.HashPassword(password));

        await cmd.ExecuteNonQueryAsync();
    }
}