namespace Grim.Token;

public class FunctionToken : IToken
{

    public readonly FunctionType Type;

    public readonly List<VariableToken> Parameters;

    public readonly IToken Body;

    public readonly int Priority;

    public readonly bool IsLeftAssociative;

    public FunctionToken(FunctionType type,List<VariableToken> parameters,IToken body,int priority)
    {
        Type = type;
        Parameters = parameters;
        Body = body;
        Priority = Math.Abs(priority);
        IsLeftAssociative = priority < 0;

        if((Type == FunctionType.Prefix || Type == FunctionType.Suffix) && Parameters.Count != 1)
            throw new Exception("prefix or suffix operator function must have one parameter.");
        
        if(Type == FunctionType.Mid && Parameters.Count != 2)
            throw new Exception("mid operator function must have two parameter.");

    }

    public override string ToString()
    {
        return nameof(FunctionToken)
               + "<" + string.Join(",",Parameters) +  ">"
               + "<" + Body + ">";
    }
}