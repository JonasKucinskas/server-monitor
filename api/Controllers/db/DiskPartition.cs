using Npgsql;

public class DiskPartition
{
    public string Name { get; set; }
    public long ReadSpeed { get; set; }
    public long WriteSpeed { get; set; }
    public long IoTime { get; set; }
    public long WeightedIoTime { get; set; }

    public async Task InsertToDatabase(NpgsqlConnection conn, DateTime time, string serverId)
    {
        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO disk_partitions (time, server_id, partition_name, read_speed, write_speed, io_time, weighted_io_time)
            VALUES (@time, @serverId, @partitionName, @readSpeed, @writeSpeed, @ioTime, @weightedIoTime);
        ", conn);

        cmd.Parameters.AddWithValue("time", time);
        cmd.Parameters.AddWithValue("serverId", serverId);
        cmd.Parameters.AddWithValue("partitionName", Name);
        cmd.Parameters.AddWithValue("readSpeed", ReadSpeed);
        cmd.Parameters.AddWithValue("writeSpeed", WriteSpeed);
        cmd.Parameters.AddWithValue("ioTime", IoTime);
        cmd.Parameters.AddWithValue("weightedIoTime", WeightedIoTime);

        await cmd.ExecuteNonQueryAsync();
    }
}
