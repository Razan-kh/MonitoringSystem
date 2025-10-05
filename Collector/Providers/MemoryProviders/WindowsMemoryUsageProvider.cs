using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MonitoringSystem.Collector.Interfaces;
using MonitoringSystem.Collector.Models;

namespace MonitoringSystem.Collector.Providers.MemoryProviders;

public class WindowsMemoryUsageProvider : IMemoryUsageProvider
{
    public (double usedMB, double availableMB) GetMemoryUsage()
    {
        try
        {
            using var availableCounter = new PerformanceCounter("Memory", "Available MBytes");
            var availableMB = availableCounter.NextValue();
            var totalMemory = GetWindowsTotalMemoryMB();
            var usedMB = totalMemory - availableMB;
            return (Math.Round(usedMB, 2), Math.Round(availableMB, 2));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Windows memory counter failed: {ex.Message}");
            return GetMemoryUsageFallback();
        }
    }

    private static (double usedMB, double availableMB) GetMemoryUsageFallback()
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

        return 8192; // 8GB default fallback
    }

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
}
