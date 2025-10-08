using System;
using System.IO;
using MonitoringSystem.Collector.Interfaces;
using MonitoringSystem.Collector.Models;

namespace MonitoringSystem.Collector.Providers.MemoryProviders;

public class LinuxMemoryUsageProvider : IMemoryUsageProvider
{
    public MemoryUsage GetMemoryUsage()
    {
        try
        {
            if (!File.Exists("/proc/meminfo"))
            {
                throw new MemoryUsageException("/proc/meminfo not found. Are you running on Linux?");
            }

            var meminfo = File.ReadAllLines("/proc/meminfo");
            double total = 0, available = 0;

            foreach (var line in meminfo)
            {
                if (line.StartsWith("MemTotal"))
                {
                    total = ParseMemValue(line);
                }
                else if (line.StartsWith("MemAvailable"))
                {
                    available = ParseMemValue(line);
                }
            }

            if (available == 0)
            {
                foreach (var line in meminfo)
                {
                    if (line.StartsWith("MemFree"))
                    {
                        available = ParseMemValue(line);
                        break;
                    }
                }

                if (available == 0)
                {
                    throw new MemoryUsageException("Could not determine available memory (MemAvailable or MemFree).");
                }
            }

            if (total == 0)
            {
                throw new MemoryUsageException("Could not determine total memory (MemTotal).");
            }

            var usedMB = total - available;
            return new MemoryUsage(Math.Round(usedMB, 2), Math.Round(available, 2));
        }
        catch (Exception ex) when (ex is not MemoryUsageException)
        {
            throw new MemoryUsageException("Failed to read Linux memory usage.", ex);
        }
    }

    private static double ParseMemValue(string line)
    {
        try
        {
            var parts = line.Split(':', StringSplitOptions.TrimEntries);
            var valuePart = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
            return double.Parse(valuePart) / 1024.0; // KB -> MB
        }
        catch (Exception ex)
        {
            throw new MemoryUsageException($"Failed to parse memory value from line: '{line}'", ex);
        }
    }
}