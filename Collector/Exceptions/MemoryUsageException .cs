namespace MonitoringSystem.Collector.Exceptions;

public class MemoryUsageException : MonitoringException
{
    public MemoryUsageException(string message, Exception inner = null) : base(message, inner) { }
}