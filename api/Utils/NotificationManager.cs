using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;

public static class NotificationManager
{
    public static List<Process> GetProcessList(Notification notification, string agentIpAddress, int agentPort)
    {
        List<Process> processes = new List<Process>();
        
        string sort = "";
        
        if (notification.resource == "ram")
        {
            sort = "mem";
        }
        else if (notification.resource == "cpu")
        {
            sort = "cpu";
        }
        else if (notification.resource == "sensors")
        {
            sort = "cpu";
        }
        else
        {
            return processes;
        }
        
        string command = $"ps -eo pid,user,cmd,time,%mem,%cpu --sort=-%{sort} | head -n 8";
        string output = SshConnection.Instance.RunCmd(agentIpAddress, agentPort, command);
        processes.AddRange(ParseProcessOutput(output, agentIpAddress, notification.id));

        return processes;
    }

    public static List<Process> ParseProcessOutput(string output, string agentIpAddress, int notificationId)
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
                notification_id = notificationId
            };

            processes.Add(processInfo);
        }

        return processes;
    }

    public static List<Notification> GetNotificationData(List<NotificationRule> rules, DataPackage data)
    {
        List<Notification> notifications = new List<Notification>();

        foreach (var rule in rules)
        {
            switch (rule.resource)
            {
                case "cpu":
                    if (data.Cpu.Cores[0].Total >= rule.usage)
                    {
                        Notification notification = new Notification()
                        {
                            notification_rule_id = rule.id,
                            resource = rule.resource,
                            usage = (float)Math.Round((double)data.Cpu.Cores[0].Total, 2)
                        };
                        notifications.Add(notification);
                    }
                    break;
                case "ram":

                    float ramUsage = data.Ram.MemUsed * 100 / data.Ram.MemTotal;
                    if (ramUsage >= rule.usage)
                    {
                        Notification notification = new Notification()
                        {
                            notification_rule_id = rule.id,
                            resource = rule.resource,
                            usage = (float)Math.Round((double)ramUsage, 2)
                        };
                        notifications.Add(notification);
                    }
                    break;
                case "disk":
                    var diskUsage = data.Disk.FreeSpace * 100 / data.Disk.TotalSpace;
                    if (diskUsage >= rule.usage)
                    {
                        Notification notification = new Notification()
                        {
                            notification_rule_id = rule.id,
                            resource = rule.resource,
                            usage = (float)Math.Round((double)diskUsage, 2)
                        };
                        notifications.Add(notification);
                    }
                    break;
                case "swap":
                    var swapUsage = data.Ram.SwapFree * 100 / data.Ram.SwapTotal;
                    if (swapUsage >= rule.usage)
                    {
                        Notification notification = new Notification()
                        {
                            notification_rule_id = rule.id,
                            resource = rule.resource,
                            usage = (float)Math.Round((double)swapUsage, 2)
                        };
                        notifications.Add(notification);
                    }
                    break;
                case "sensors":
                    var sensorUsage = data.SensorList.Sensors[0].Value;
                    if (sensorUsage >= rule.usage)
                    {
                        Notification notification = new Notification()
                        {
                            notification_rule_id = rule.id,
                            resource = rule.resource,
                            usage = sensorUsage
                        };
                        notifications.Add(notification);
                    }
                    break;
                case "battery":
                    var batteryUsage = data.Battery.Capacity;
                    if (batteryUsage >= rule.usage)
                    {
                        Notification notification = new Notification()
                        {
                            notification_rule_id = rule.id,
                            resource = rule.resource,
                            usage = batteryUsage
                        };
                        notifications.Add(notification);
                    }
                    break;
                default:
                    break;
            }
        }

        return notifications;
    }
}