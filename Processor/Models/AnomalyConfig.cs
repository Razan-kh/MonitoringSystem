namespace MonitoringSystem.Processor.Models;

public class AnomalyConfig
{
    public double MemoryUsageAnomalyThresholdPercentage { get; init; }
    public double CpuUsageAnomalyThresholdPercentage { get; init; }
    public double MemoryUsageThresholdPercentage { get; init; }
    public double CpuUsageThresholdPercentage { get; init; }
}