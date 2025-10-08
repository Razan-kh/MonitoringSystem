public class MonitoringException : Exception
{
    public MonitoringException(string message, Exception inner = null) : base(message, inner) { }
}