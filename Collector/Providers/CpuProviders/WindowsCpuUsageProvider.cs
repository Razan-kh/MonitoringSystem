using System;
using System.Diagnostics;
using System.Threading;
using MonitoringSystem.Collector.Interfaces;

namespace MonitoringSystem.Collector.Providers.CpuProviders;

public class WindowsCpuUsageProvider : ICpuUsageProvider
{
    public double GetCpuUsage()
    {
        try
        {
            using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ = cpuCounter.NextValue();
            Thread.Sleep(1000);
            return Math.Round(cpuCounter.NextValue(), 2);
        }
        catch (Exception ex)
        {
            throw new CpuUsageException("Failed to read Windows CPU usage.", ex);
        }
    }
}