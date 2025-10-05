using System;
using MonitoringSystem.Collector.Interfaces;

namespace MonitoringSystem.Collector.Providers.MemoryProviders;

public class UnsupportedMemoryUsageProvider : IMemoryUsageProvider
{
    public (double usedMB, double availableMB) GetMemoryUsage()
    {
        Console.WriteLine("Memory usage not supported on this OS.");
        return (0, 0);
    }
}
