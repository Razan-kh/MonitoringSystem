namespace MonitoringSystem.Collector.Interfaces;

public interface IMemoryUsageProvider
{
    (double usedMB, double availableMB) GetMemoryUsage();
}