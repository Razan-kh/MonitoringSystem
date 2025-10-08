using System;
using System.Runtime.InteropServices;
using MonitoringSystem.Collector.Interfaces;
using MonitoringSystem.Collector.Models;
using MonitoringSystem.Collector.Providers.MemoryProviders;

namespace MonitoringSystem.Collector.Services;

public class MemoryUsageCollector
{
    private readonly IMemoryUsageProvider _provider;

    public MemoryUsageCollector()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _provider = new WindowsMemoryUsageProvider();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _provider = new LinuxMemoryUsageProvider();
        }
        else
        {
            throw new PlatformNotSupportedException(
                $"Memory usage collection is not supported on this platform: {RuntimeInformation.OSDescription}");
        }
    }

    public MemoryUsage GetMemoryUsage()
    {
        try
        {
            return _provider.GetMemoryUsage();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get memory usage.", ex);
        }
    }
}