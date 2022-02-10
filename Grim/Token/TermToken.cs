namespace grim_interpreter.Token;

public class TermToken : ExpressionToken
{

    public readonly List<ExpressionToken> Expressions;

    public TermToken(List<ExpressionToken> expressions)
    {
        Expressions = expressions;
    }

    public override string ToString()
    {
        return nameof(TermToken) + "<" + 
               string.Join(",", Expressions.Select(e=>e.ToString()).ToList())
               + ">";
    }
}