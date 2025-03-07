using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Text.RegularExpressions;

namespace monitor
{
    public class Cpu
    {
        public string Name { get; set; }
        public List<CoreMetrics> Cores { get; set; }
        public double Freq { get; set; }

        public Cpu()
        {
        }
    }
    public class CoreMetrics
    {
        public string CoreName;
        public double Total; 
        public double User; 
        public double Nice; 
        public double System; 
        public double Idle; 
        public double IOWait; 
        public double IRQ; 
        public double SoftIRQ; 
        public double Steal; 

        public CoreMetrics()
        {
            
        }
    }
}