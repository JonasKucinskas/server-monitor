public class SensorList
{
    public List<Sensor> Sensors;
    public SensorList()
    {
        Sensors = [];
    }
    
    public void ClearMetrics()
    {
        Sensors = [];
    }

    public SensorList Clone()
    {
        return new SensorList
        {
            Sensors = this.Sensors
        };
    }

    public void GatherMetrics()
    {
        string line = File.ReadLines("/sys/class/thermal/thermal_zone0/temp").First();
        int value = int.Parse(line);
        Sensor cpu = new Sensor
        {
            Name = "Processor",
            value = value / 1000
        };
        this.Sensors.Add(cpu);
    }
}

public class Sensor
{   
    required public string Name;
    public int value; //c
}
