using System;

namespace MonitoringSystem.Shared.Models;

public class AlertMessage
{
    public string ServerIdentifier { get; set; }
    public AnomalyType Type { get; set; } // Anomaly | HighUsage
    public string Description { get; set; }
    public ServerStatistics Stats { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}