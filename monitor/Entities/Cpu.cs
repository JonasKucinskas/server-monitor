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
            this.Cores = [];
            this.Name = GetCpuName();
        }

        public void ClearMetrics()
        {
            this.Cores = [];
        }

        public Cpu Clone()
        {
            return new Cpu
            {
                Name = this.Name,
                Cores = this.Cores,
                Freq = this.Freq
            };
        }

        static string GetCpuName()
        {
            foreach (var line in File.ReadLines("/proc/cpuinfo"))
            {
                if (line.StartsWith("model name"))
                {
                    return Regex.Replace(line, @"^\s*model name\s*:\s*", "");
                }
            }
            return null;
        }

        static double GetFrequency()
        {
            string line = File.ReadLines("/sys/devices/system/cpu/cpu0/cpufreq/scaling_cur_freq").First();
            return int.Parse(line) / 1000000.0;
        }

        public async Task GatherMetrics()
        {
            var sample1 = CpuTimes.SampleCpuTimes();
            await Task.Delay(1000);
            var sample2 = CpuTimes.SampleCpuTimes();

            foreach (var kv in sample1)
            {
                string coreName = kv.Key;
                if (!sample2.ContainsKey(coreName))
                {
                    continue; // Skip if for some reason the second sample doesn't have this core.
                }

                CpuTimes t1 = sample1[coreName];
                CpuTimes t2 = sample2[coreName];
                
                this.Cores.Add(new CoreMetrics(t1, t2));
            }

            this.Freq = GetFrequency();
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

        public CoreMetrics(CpuTimes t1, CpuTimes t2)
        {
            // Compute differences for each metric.
            long userDiff = t2.User - t1.User;
            long niceDiff = t2.Nice - t1.Nice;
            long systemDiff = t2.System - t1.System;
            long idleDiff = t2.Idle - t1.Idle;
            long iowaitDiff = t2.IOWait - t1.IOWait;
            long irqDiff = t2.IRQ - t1.IRQ;
            long softirqDiff = t2.SoftIRQ - t1.SoftIRQ;
            long stealDiff = t2.Steal - t1.Steal;
            long totalDiff = t2.Total - t1.Total;
            
            if (totalDiff == 0)
            {
                Console.WriteLine($"{t1.Name}: No change detected.");
                return;
            }

            //Calculate percentages.
            this.CoreName = t1.Name;
            this.User = userDiff * 100.0 / totalDiff;
            this.Total = (totalDiff - (t2.IdleAll - t1.IdleAll)) * 100.0 / totalDiff;
            this.Nice = niceDiff * 100.0 / totalDiff;
            this.System = systemDiff * 100.0 / totalDiff;
            this.Idle = idleDiff * 100.0 / totalDiff;
            this.IOWait = iowaitDiff * 100.0 / totalDiff;
            this.IRQ = irqDiff * 100.0 / totalDiff;
            this.SoftIRQ = softirqDiff * 100.0 / totalDiff;
            this.Steal = stealDiff * 100.0 / totalDiff;
        }
    }

    public class CpuTimes
    {
        public string Name { get; private set; }
        public long User { get; private set; }
        public long Nice { get; private set; }
        public long System { get; private set; }
        public long Idle { get; private set; }
        public long IOWait { get; private set; }
        public long IRQ { get; private set; }
        public long SoftIRQ { get; private set; }
        public long Steal { get; private set; }

        // Total time is the sum of the fields we consider
        public long Total => User + Nice + System + Idle + IOWait + IRQ + SoftIRQ + Steal;
        // Idle time is the sum of idle and iowait
        public long IdleAll => Idle + IOWait;

        public static Dictionary<string, CpuTimes> SampleCpuTimes()
        {
            Dictionary<string, CpuTimes> cpus = [];

            bool cpuLineRead = false;

            // Read all lines from /proc/stat
            foreach (var line in File.ReadLines("/proc/stat"))
            {
                // We are interested in lines that start with "cpu" followed by a number.
                if (line.StartsWith("cpu"))
                {
                    cpuLineRead = true;
                    var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    // tokens[0] is the CPU name (e.g., "cpu0", "cpu1", etc.)
                    var cpu = new CpuTimes
                    {
                        Name = tokens[0],
                        User = long.Parse(tokens[1]),
                        Nice = long.Parse(tokens[2]),
                        System = long.Parse(tokens[3]),
                        Idle = long.Parse(tokens[4]),
                        IOWait = long.Parse(tokens[5]),
                        IRQ = long.Parse(tokens[6]),
                        SoftIRQ = long.Parse(tokens[7]),
                        Steal = tokens.Length > 8 ? long.Parse(tokens[8]) : 0
                    };

                    cpus[cpu.Name] = cpu;
                }
                else if (cpuLineRead)
                {
                    //end of cpu lines.
                    break;
                }
            }
            return cpus;
        }
    }
}