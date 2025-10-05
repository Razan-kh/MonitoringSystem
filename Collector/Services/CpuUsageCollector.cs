using System;
using System.Runtime.InteropServices;
using MonitoringSystem.Collector.Providers;
using MonitoringSystem.Collector.Interfaces;
using MonitoringSystem.Collector.Providers.CpuProviders;

namespace MonitoringSystem.Collector.Services;

public class CpuUsageCollector
{
    private readonly ICpuUsageProvider _provider;

    public CpuUsageCollector()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _provider = new WindowsCpuUsageProvider();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _provider = new LinuxCpuUsageProvider();
        }
        else
        {
            _provider = new UnsupportedCpuUsageProvider();
        }
    }

    public double GetCpuUsage() => _provider.GetCpuUsage();
}