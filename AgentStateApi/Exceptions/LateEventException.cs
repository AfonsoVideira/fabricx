namespace AgentStateApi.Exceptions;

public class LateEventException : Exception
{
    public LateEventException(string message) : base(message)
    {
    }
    
    public LateEventException(string message, Exception innerException) : base(message, innerException)
    {
    }
} 