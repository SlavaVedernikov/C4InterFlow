namespace C4InterFlow.Visualization.Plantuml;

/// <summary>
/// PlantumlException
/// </summary>
public class PlantumlException : Exception
{
    public PlantumlException(string message) : base(message)
    {
    }

    public PlantumlException(string message, Exception innerException) : base(message, innerException)
    {
    }
}