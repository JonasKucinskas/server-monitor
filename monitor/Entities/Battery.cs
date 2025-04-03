public class Battery
{
    public string Name;
    public int Capacity;
    public string Status;

    public Battery()
    {
        Name = GetName();
    }

    public Battery Clone()
    {
        return new Battery
        {
            Name = this.Name,
            Capacity = this.Capacity,
            Status = this.Status
        };
    }

    private static string GetName()
    {
        string parentDirectory = "/sys/class/power_supply/";
        string searchSubstring = "BAT";
        string[] directories = Directory.GetDirectories(parentDirectory);
        foreach (string directory in directories)
        {
            // Extract just the directory name from the full path.
            string dirName = Path.GetFileName(directory);
            if (dirName.Contains(searchSubstring))
            {
                return dirName;
            }
        }
        
        return null;
    }

    public void GatherMetrics()
    {
        if (Name == "")
        {
            Console.WriteLine("Battery's name not set");
            return;
        }
        this.Capacity = GetCapacity();
        this.Status = GetStatus();
    }

    private int GetCapacity()
    {
        string line = File.ReadLines($"/sys/class/power_supply/{this.Name}/capacity").First();
        return int.Parse(line);
    }

    private string GetStatus()
    {
        return File.ReadLines($"/sys/class/power_supply/{this.Name}/status").First();
    }
 
}