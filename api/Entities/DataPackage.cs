using monitor;

public class DataPackage
{
    required public Cpu Cpu {get; set;}
    required public Battery Battery {get; set;}
    required public Disk Disk {get; set;}
    required public Network Network {get; set;}
    required public Ram Ram {get; set;}
    required public SensorList SensorList {get; set;}
}