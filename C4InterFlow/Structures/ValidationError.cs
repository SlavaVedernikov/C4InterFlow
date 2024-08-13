namespace C4InterFlow.Structures;

public struct ValidationError
{
    public string Template { get; }
    public object[] Args { get; }

    public ValidationError(string template, params object[] args)
    {
        Template = template;
        Args = args;
    }
}