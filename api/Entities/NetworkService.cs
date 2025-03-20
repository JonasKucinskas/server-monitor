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
}


