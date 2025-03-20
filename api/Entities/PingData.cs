public class PingData
{
    public int id { get; set; }
    public int serviceId { get; set; }
    public bool isUp { get; set; } 
    public float responseTime { get; set; }
    public string errorMessage { get; set; }
    public DateTime timestamp { get; set; }    
}