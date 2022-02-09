public class FunctionCallToken : ExpressionToken
{
    public readonly string Name;

    public readonly TermToken Parameters;

    public FunctionCallToken(string name,TermToken parameters)
    {
        Name = name;
        Parameters = parameters;
    }

    public override string ToString()
    {
        return nameof(FunctionCallToken) + $"<{Name}><{Parameters}>";
    }
}