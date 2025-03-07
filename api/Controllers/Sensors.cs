using Npgsql;

public class SensorList
{
    public List<Sensor> Sensors;
    public SensorList()
    {
        Sensors = [];
    }


}
public class Sensor
{   
    required public string Name;
    public int value; //c

    public async Task InsertToDatabase(NpgsqlConnection conn, DateTime time, string serverId)
    {
        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO sensors (time, server_id, sensor_name, value)
            VALUES (@time, @serverId, @sensorName, @value);
        ", conn);

        cmd.Parameters.AddWithValue("time", time);
        cmd.Parameters.AddWithValue("serverId", serverId);
        cmd.Parameters.AddWithValue("sensorName", Name);
        cmd.Parameters.AddWithValue("value", value);

        await cmd.ExecuteNonQueryAsync();
    }
}