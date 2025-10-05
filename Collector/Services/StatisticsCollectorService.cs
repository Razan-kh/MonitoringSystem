using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using MonitoringSystem.Shared.Models;
using MonitoringSystem.Shared.Interfaces;
using MonitoringSystem.Collector.Models;

namespace MonitoringSystem.Collector.Services;

public class StatisticsCollectorService : BackgroundService
{
    private readonly IMessagePublisher _publisher;
    private readonly CollectorConfig _config;
    private readonly CpuUsageCollector _cpuCollector;
    private readonly MemoryUsageCollector _memoryCollector;

    public StatisticsCollectorService(IMessagePublisher publisher, IOptions<CollectorConfig> options, CpuUsageCollector cpuCollector,
            MemoryUsageCollector memoryCollector)
    {
        _publisher = publisher;
        _config = options.Value;
        _cpuCollector = cpuCollector;
        _memoryCollector = memoryCollector;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var stats = Collect();
                var topic = $"ServerStatistics.{_config.ServerIdentifier}";
                var payload = JsonConvert.SerializeObject(stats);
                var exchange = "ServerStatsExchange";
                await _publisher.PublishAsync(topic, payload, exchange);
                Console.WriteLine($"Published stats for {_config.ServerIdentifier} at {DateTime.UtcNow}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error collecting statistics: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(_config.SamplingIntervalSeconds), stoppingToken);
        }
    }

    private ServerStatistics Collect()
    {
        var cpu = _cpuCollector.GetCpuUsage();
        var (used, available) = _memoryCollector.GetMemoryUsage();

        return new ServerStatistics
        {
            ServerIdentifier = _config.ServerIdentifier,
            MemoryUsage = used,
            AvailableMemory = available,
            CpuUsage = cpu,
            Timestamp = DateTime.UtcNow
        };
    }
}
/*
    public static double GetCpuUsage()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowsCpuUsage();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return GetLinuxCpuUsage();
        }
        else
        {
            Console.WriteLine("CPU usage not supported on this OS.");
            return 0.0;
        }
    }

    private static double GetWindowsCpuUsage()
    {
        try
        {
            using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ = cpuCounter.NextValue(); // first reading is always 0
            Thread.Sleep(1000);
            return Math.Round(cpuCounter.NextValue(), 2);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Windows CPU counter failed: {ex.Message}");
            return GetCpuUsageFallback();
        }
    }

    private static double GetLinuxCpuUsage()
    {
        try
        {
            var lines = File.ReadAllLines("/proc/stat");
            var cpuLine = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            ulong user = ulong.Parse(cpuLine[1]);
            ulong nice = ulong.Parse(cpuLine[2]);
            ulong system = ulong.Parse(cpuLine[3]);
            ulong idle = ulong.Parse(cpuLine[4]);

            ulong total1 = user + nice + system + idle;

            Thread.Sleep(500);

            lines = File.ReadAllLines("/proc/stat");
            cpuLine = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            ulong user2 = ulong.Parse(cpuLine[1]);
            ulong nice2 = ulong.Parse(cpuLine[2]);
            ulong system2 = ulong.Parse(cpuLine[3]);
            ulong idle2 = ulong.Parse(cpuLine[4]);

            ulong total2 = user2 + nice2 + system2 + idle2;

            ulong totalDelta = total2 - total1;
            ulong idleDelta = idle2 - idle;

            double cpuUsage = 100.0 * (totalDelta - idleDelta) / totalDelta;
            return Math.Round(cpuUsage, 2);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Linux CPU usage failed: {ex.Message}");
            return GetCpuUsageFallback();
        }
    }

    private static double GetCpuUsageFallback()
    {
        // Simple fallback - return a default value
        return 0.0;
    }

    // ---------------------------
    // Memory Usage
    // ---------------------------
    public static (double usedMB, double availableMB) GetMemoryUsage()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowsMemoryUsage();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return GetLinuxMemoryUsage();
        }
        else
        {
            Console.WriteLine("Memory usage not supported on this OS.");
            return (0, 0);
        }
    }

    // Windows Memory Usage using Performance Counters
    private static (double usedMB, double availableMB) GetWindowsMemoryUsage()
    {
        try
        {
            using var availableCounter = new PerformanceCounter("Memory", "Available MBytes");
            var availableMB = availableCounter.NextValue();
            
            // Get total memory using Windows API
            var totalMemory = GetWindowsTotalMemoryMB();
            var usedMB = totalMemory - availableMB;
            
            return (Math.Round(usedMB, 2), Math.Round(availableMB, 2));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Windows memory counter failed: {ex.Message}");
            // Fallback using GlobalMemoryStatusEx
            return GetWindowsMemoryUsageFallback();
        }
    }

    // Fallback method using Windows API
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private sealed class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
        
        public MEMORYSTATUSEX()
        {
            dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        }
    }

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    private static (double usedMB, double availableMB) GetWindowsMemoryUsageFallback()
    {
        try
        {
            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                var totalMB = memStatus.ullTotalPhys / (1024.0 * 1024.0);
                var availableMB = memStatus.ullAvailPhys / (1024.0 * 1024.0);
                var usedMB = totalMB - availableMB;
                return (Math.Round(usedMB, 2), Math.Round(availableMB, 2));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Windows memory API failed: {ex.Message}");
        }
        
        // Final fallback
        return (0, 0);
    }

    private static double GetWindowsTotalMemoryMB()
    {
        try
        {
            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                return memStatus.ullTotalPhys / (1024.0 * 1024.0);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get total memory: {ex.Message}");
        }
        
        return 8192; // 8GB default
    }

    private static (double usedMB, double availableMB) GetLinuxMemoryUsage()
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

            // If MemAvailable is not found, calculate from MemFree
            if (available == 0)
            {
                double free = 0;
                foreach (var line in meminfo)
                {
                    if (line.StartsWith("MemFree"))
                        free = double.Parse(line.Split(':')[1].Trim().Split(' ')[0]) / 1024.0;
                }
                available = free;
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
*/