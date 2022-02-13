namespace Grim.Token;

public class TermToken : IToken
{

    public readonly List<IToken> Expressions;

    public TermToken(List<IToken> expressions)
    {
        Expressions = expressions;
    }

    public override string ToString()
    {
        return nameof(TermToken) + "<" + 
               string.Join(",", Expressions)
               + ">";
    }
}