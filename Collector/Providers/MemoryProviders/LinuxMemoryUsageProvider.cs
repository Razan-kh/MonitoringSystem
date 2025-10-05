using System;
using System.IO;
using MonitoringSystem.Collector.Interfaces;

namespace MonitoringSystem.Collector.Providers.MemoryProviders;

public class LinuxMemoryUsageProvider : IMemoryUsageProvider
{
    public (double usedMB, double availableMB) GetMemoryUsage()
    {
        try
        {
            var meminfo = File.ReadAllLines("/proc/meminfo");
            double total = 0, available = 0;

            foreach (var line in meminfo)
            {
                if (line.StartsWith("MemTotal"))
                    total = double.Parse(line.Split(':')[1].Trim().Split(' ')[0]) / 1024.0;
                if (line.StartsWith("MemAvailable"))
                    available = double.Parse(line.Split(':')[1].Trim().Split(' ')[0]) / 1024.0;
            }

            // Fallback to MemFree if MemAvailable is not present
            if (available == 0)
            {
                foreach (var line in meminfo)
                {
                    if (line.StartsWith("MemFree"))
                    {
                        available = double.Parse(line.Split(':')[1].Trim().Split(' ')[0]) / 1024.0;
                        break;
                    }
                }
            }

            var usedMB = total - available;
            return (Math.Round(usedMB, 2), Math.Round(available, 2));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Linux memory usage failed: {ex.Message}");
            return (0, 0);
        }
    }
}