namespace Grim.Token;

public class FunctionCallToken : IToken
{
    public readonly IToken Function;

    public readonly List<IToken> Parameters;

    public FunctionCallToken(IToken function, List<IToken> parameters)
    {
        Function = function;
        Parameters = parameters;
    }

    public override string ToString()
    {
        return nameof(FunctionCallToken) + $"<{Function}><{string.Join(",", Parameters)}>";
    }
}