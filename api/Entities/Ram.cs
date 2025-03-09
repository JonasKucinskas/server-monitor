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
    }
}