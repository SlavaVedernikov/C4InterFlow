namespace C4InterFlow.Commons;

public class LogMessage
{
    public string Template { get; }
    public object[] Args { get; }

    public LogMessage(string template, params object[] args)
    {
        Template = template;
        Args = args;
    }
}