using Grim.VM;

namespace Grim.Token;

public class FunctionToken : IVariable,ExpressionToken
{

    public readonly FunctionType Type;

    public readonly List<VariableToken> Parameters;

    public readonly TermToken Body;

    public readonly int Priority;

    public FunctionToken(FunctionType type,List<VariableToken> parameters,TermToken body,int priority)
    {
        Type = type;
        Parameters = parameters;
        Body = body;
        Priority = priority;
        Check();
    }

    private void Check()
    {
        if((Type == FunctionType.Prefix || Type == FunctionType.Suffix) && Parameters.Count != 1)
            throw new Exception("prefix or suffix operator function must have one parameter.");
        
        if(Type == FunctionType.Mid && Parameters.Count != 2)
            throw new Exception("mid operator function must have two parameter.");
    }

    public override string ToString()
    {
        return nameof(FunctionToken)
               + "<" + string.Join(",",Parameters.Select(v=>v.ToString())) +  ">"
               + "<" + Body + ">";
    }
}