using System;
using MonitoringSystem.Collector.Interfaces;

namespace MonitoringSystem.Collector.Providers.CpuProviders;

public class UnsupportedCpuUsageProvider : ICpuUsageProvider
{
    public double GetCpuUsage()
    {
        Console.WriteLine("CPU usage not supported on this OS.");
        return 0.0;
    }
}