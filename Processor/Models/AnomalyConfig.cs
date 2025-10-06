namespace MonitoringSystem.Processor.Models;

public class AnomalyConfig
{
    public double MemoryUsageAnomalyThresholdPercentage { get; set; }
    public double CpuUsageAnomalyThresholdPercentage { get; set; }
    public double MemoryUsageThresholdPercentage { get; set; }
    public double CpuUsageThresholdPercentage { get; set; }
}