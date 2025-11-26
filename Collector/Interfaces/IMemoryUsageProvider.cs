using MonitoringSystem.Collector.Models;

namespace MonitoringSystem.Collector.Interfaces;

public interface IMemoryUsageProvider
{
    MemoryUsage GetMemoryUsage();
}