namespace Grim.Token;

public class FunctionCallToken : ExpressionToken
{
    public readonly ExpressionToken Function;

    public readonly TermToken Parameters;

    public FunctionCallToken(ExpressionToken function, TermToken parameters)
    {
        Function = function;
        Parameters = parameters;
    }

    public override string ToString()
    {
        return nameof(FunctionCallToken) + $"<{Function}><{Parameters}>";
    }
}