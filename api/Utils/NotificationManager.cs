using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;

public static class NotificationManager
{
    public static List<Process> GetProcessList(List<NotificationRule> rules, DataPackage data, string agentIpAddress, int agentPort)
    {
        return GetProcessOutputs(rules, data, agentIpAddress, agentPort);
    }

    public static List<Process> ParseProcessOutput(string output, string agentIpAddress, int ruleId)
    {
        List<Process> processes = new List<Process>();

        var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 6)
                continue;

            if (!int.TryParse(parts[0], out int pid))
            {
                Console.WriteLine("Failed to parse pid");
                continue;
            }

            if (!float.TryParse(parts[parts.Length - 2], NumberStyles.Float, CultureInfo.InvariantCulture, out float ramUsage))
            {
                Console.WriteLine("Failed to parse ramusage");
                continue;
            }
                

            if (!float.TryParse(parts[parts.Length - 1], NumberStyles.Float, CultureInfo.InvariantCulture, out float cpuUsage))
            {
                Console.WriteLine("Failed to parse cpuUsage");
                continue;
            }

            if (!TimeOnly.TryParseExact(parts[parts.Length - 3], "HH:mm:ss", out TimeOnly processTime))
            {
                Console.WriteLine("Failed to parse processTime");
                continue;
            }

            var processInfo = new Process
            {
                pid = pid,
                process_user = parts[1],
                name = string.Join(" ", parts[2..^3]),
                process_time = processTime,
                ram_usage = (float)Math.Round((double)ramUsage, 2),  
                cpu_usage = (float)Math.Round((double)cpuUsage, 2),  
                system_ip = agentIpAddress,
                notification_rule_id = ruleId
            };

            processes.Add(processInfo);
        }

        return processes;
    }

    public static List<Process> GetProcessOutputs(List<NotificationRule> rules, DataPackage data, string agentIpAddress, int agentPort)
    {
        List<Process> processes = new List<Process>();

        foreach (var rule in rules)
        {
            string command = GetCommandForRule(rule, data);

            if (!string.IsNullOrEmpty(command))
            {
                string output = SshConnection.Instance.RunCmd(agentIpAddress, agentPort, command);
                processes.AddRange(ParseProcessOutput(output, agentIpAddress, rule.id));
            }
        }

        return processes;
    }

    private static string GetCommandForRule(NotificationRule rule, DataPackage data)
    {
        switch (rule.resource)
        {
            case "cpu":
                if (data.Cpu.Cores[0].Total >= rule.usage)
                    return "ps -eo pid,user,cmd,time,%mem,%cpu --sort=-%cpu | head -n 8";
                break;
            case "mem":
                float ramUsage = data.Ram.MemUsed * 100 / data.Ram.MemTotal;
                if (ramUsage >= rule.usage)
                    return "ps -eo pid,user,cmd,time,%mem,%cpu --sort=-%mem | head -n 8";
                break;
            default:
                break;
        }
        return string.Empty;
    }
}