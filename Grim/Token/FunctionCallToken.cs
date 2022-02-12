namespace Grim.Token;

public class FunctionCallToken : ExpressionToken
{
    public readonly ExpressionToken Function;

    public readonly List<ExpressionToken> Parameters;

    public FunctionCallToken(ExpressionToken function, List<ExpressionToken> parameters)
    {
        Function = function;
        Parameters = parameters;
    }

    public override string ToString()
    {
        return nameof(FunctionCallToken) + $"<{Function}><{string.Join(",", Parameters)}>";
    }
}