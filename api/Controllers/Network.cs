using Npgsql;
public class Network
{
    public List<NetworkMetric> Metrics { get; set; }
    public Network()
    {
        Metrics = [];
    }
}

public class NetworkMetric
{
    //b/s
    public string Name;
    public long Upload;
    public long Download;
    public NetworkInterfaceData Iface;
    public NetworkMetric()
    {
        
    }

    public async Task InsertToDatabase(NpgsqlConnection conn, DateTime time, string serverId)
    {
        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO network_metrics (time, server_id, interface_name, upload, download, receive_bytes, receive_packets, receive_errs, receive_drop, receive_fifo, receive_frame, receive_compressed, receive_multicast, transmit_bytes, transmit_packets, transmit_errs, transmit_drop, transmit_fifo, transmit_colls, transmit_carrier, transmit_compressed)
            VALUES (@time, @serverId, @interfaceName, @upload, @download, @receiveBytes, @receivePackets, @receiveErrs, @receiveDrop, @receiveFifo, @receiveFrame, @receiveCompressed, @receiveMulticast, @transmitBytes, @transmitPackets, @transmitErrs, @transmitDrop, @transmitFifo, @transmitColls, @transmitCarrier, @transmitCompressed);
        ", conn);

        cmd.Parameters.AddWithValue("time", time);
        cmd.Parameters.AddWithValue("serverId", serverId);
        cmd.Parameters.AddWithValue("interfaceName", Name);
        cmd.Parameters.AddWithValue("upload", Upload);
        cmd.Parameters.AddWithValue("download", Download);
        cmd.Parameters.AddWithValue("receiveBytes", Iface.ReceiveBytes);
        cmd.Parameters.AddWithValue("receivePackets", Iface.ReceivePackets);
        cmd.Parameters.AddWithValue("receiveErrs", Iface.ReceiveErrs);
        cmd.Parameters.AddWithValue("receiveDrop", Iface.ReceiveDrop);
        cmd.Parameters.AddWithValue("receiveFifo", Iface.ReceiveFifo);
        cmd.Parameters.AddWithValue("receiveFrame", Iface.ReceiveFrame);
        cmd.Parameters.AddWithValue("receiveCompressed", Iface.ReceiveCompressed);
        cmd.Parameters.AddWithValue("receiveMulticast", Iface.ReceiveMulticast);
        cmd.Parameters.AddWithValue("transmitBytes", Iface.TransmitBytes);
        cmd.Parameters.AddWithValue("transmitPackets", Iface.TransmitPackets);
        cmd.Parameters.AddWithValue("transmitErrs", Iface.TransmitErrs);
        cmd.Parameters.AddWithValue("transmitDrop", Iface.TransmitDrop);
        cmd.Parameters.AddWithValue("transmitFifo", Iface.TransmitFifo);
        cmd.Parameters.AddWithValue("transmitColls", Iface.TransmitColls);
        cmd.Parameters.AddWithValue("transmitCarrier", Iface.TransmitCarrier);
        cmd.Parameters.AddWithValue("transmitCompressed", Iface.TransmitCompressed);

        await cmd.ExecuteNonQueryAsync();
    }
}
public class NetworkInterfaceData
{
    public string Name { get; private set; }
    // Receive columns
    public long ReceiveBytes { get; private set; }
    public long ReceivePackets { get; private set; }
    public long ReceiveErrs { get; private set; }
    public long ReceiveDrop { get; private set; }
    public long ReceiveFifo { get; private set; }
    public long ReceiveFrame { get; private set; }
    public long ReceiveCompressed { get; private set; }
    public long ReceiveMulticast { get; private set; }
    // Transmit columns
    public long TransmitBytes { get; private set; }
    public long TransmitPackets { get; private set; }
    public long TransmitErrs { get; private set; }
    public long TransmitDrop { get; private set; }
    public long TransmitFifo { get; private set; }
    public long TransmitColls { get; private set; }
    public long TransmitCarrier { get; private set; }
    public long TransmitCompressed { get; private set; }
}
