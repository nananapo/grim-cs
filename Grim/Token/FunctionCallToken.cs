namespace Grim.Token;

public class FunctionCallToken : ExpressionToken
{
    public readonly TermToken Function;

    public readonly TermToken Parameters;

    public FunctionCallToken(TermToken function, TermToken parameters)
    {
        Function = function;
        Parameters = parameters;
    }

    public override string ToString()
    {
        return nameof(FunctionCallToken) + $"<{Function}><{Parameters}>";
    }
}