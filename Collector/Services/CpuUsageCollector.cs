using System;
using System.Runtime.InteropServices;
using MonitoringSystem.Collector.Interfaces;
using MonitoringSystem.Collector.Exceptions;
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
            throw new PlatformNotSupportedException(
                $"CPU usage collection is not supported on this platform: {RuntimeInformation.OSDescription}");
        }
    }

    public double GetCpuUsage()
    {
        try
        {
            return _provider.GetCpuUsage();
        }
        catch (MonitoringException ex)
        {
            throw new CpuUsageException("Failed to read Windows memory usage.", ex);
        }
    }
}