using System;
using System.Runtime.InteropServices;
using MonitoringSystem.Collector.Interfaces;
using MonitoringSystem.Collector.Providers;
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
            _provider = new UnsupportedMemoryUsageProvider();
        }
    }

    public (double usedMB, double availableMB) GetMemoryUsage() => _provider.GetMemoryUsage();
}