using System;
using MongoDB.Bson.Serialization.Attributes;  // <-- add this

namespace MonitoringSystem.Shared.Models;

[BsonIgnoreExtraElements]
public class ServerStatistics
{
    public string ServerIdentifier { get; set; }
    public double MemoryUsage { get; set; } // MB
    public double AvailableMemory { get; set; } // MB
    public double CpuUsage { get; set; } // percentage [0..100]
    public DateTime Timestamp { get; set; }
}