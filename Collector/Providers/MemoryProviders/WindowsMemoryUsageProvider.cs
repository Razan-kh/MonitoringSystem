using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MonitoringSystem.Collector.Interfaces;
using MonitoringSystem.Collector.Models;

namespace MonitoringSystem.Collector.Providers.MemoryProviders;

public class WindowsMemoryUsageProvider : IMemoryUsageProvider
{
    public MemoryUsage GetMemoryUsage()
    {
        try
        {
            using var availableCounter = new PerformanceCounter("Memory", "Available MBytes");
            var availableMB = availableCounter.NextValue();
            var totalMemory = GetWindowsTotalMemoryMB();
            var usedMB = totalMemory - availableMB;

            if (usedMB < 0)
            {
                throw new MemoryUsageException("Used memory calculated as negative.");
            }

            return new MemoryUsage(Math.Round(usedMB, 2), Math.Round(availableMB, 2));
        }
        catch (Exception ex)
        {
            throw new MemoryUsageException("Failed to read Windows memory usage.", ex);
        }
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
            throw new MemoryUsageException("Failed to read Windows memory usage.", ex);
        }

        return 8192;
    }

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
}