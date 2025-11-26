namespace MonitoringSystem.Collector.Exceptions;

public class CpuUsageException : MonitoringException
{
    public CpuUsageException(string message, Exception inner = null) : base(message, inner) { }
}