using System.Text.RegularExpressions;

namespace monitor
{
    public class Ram
    {
        //memory data in Kb.
        public int MemTotal {get; set;}
        public int MemFree {get; set;}
        public int MemUsed => MemTotal - MemFree;
        public int MemAvailable {get; set;}
        public int Buffers {get; set;}
        public int Cached {get; set;}
        public int SwapTotal {get; set;}
        public int SwapFree {get; set;}
        public int SwapUsed => SwapTotal - SwapFree;
        
        public Ram Clone()
        {
            return new Ram
            {
                MemTotal = this.MemTotal,
                MemFree = this.MemFree,
                MemAvailable = this.MemAvailable,
                Buffers = this.Buffers,
                Cached = this.Cached,
                SwapTotal = this.SwapTotal,
                SwapFree = this.SwapFree
            };
        }

        public void GatherMetrics()
        {
            foreach (var line in File.ReadLines("/proc/meminfo"))
            {
                if (line.StartsWith("MemTotal"))
                {
                    this.MemTotal = ExtractMemoryValue(line);
                }
                else if (line.StartsWith("MemFree"))
                {
                    this.MemFree = ExtractMemoryValue(line);
                }
                else if (line.StartsWith("MemAvailable"))
                {
                    this.MemAvailable = ExtractMemoryValue(line);
                }
                else if (line.StartsWith("Buffers"))
                {
                    this.Buffers = ExtractMemoryValue(line);
                }
                else if (line.StartsWith("Cached"))
                {
                    this.Cached = ExtractMemoryValue(line);
                }
                else if (line.StartsWith("SwapTotal"))
                {
                    this.SwapTotal = ExtractMemoryValue(line);
                }
                else if (line.StartsWith("SwapFree"))
                {
                    this.SwapFree = ExtractMemoryValue(line);
                }
            }
        }

        private static int ExtractMemoryValue(string line)
        {
            Match match = Regex.Match(line, @"\d+");
            return match.Success ? int.Parse(match.Value) : 0;
        }
    }
}