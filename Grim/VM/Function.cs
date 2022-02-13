using Grim.Token;

namespace Grim.VM;

public class Function : IVariable
{
    public readonly Scope DefinedScope;

    public readonly FunctionToken FunctionToken;
    
    public FunctionType Type => FunctionToken.Type;

    public List<VariableToken> Parameters => FunctionToken.Parameters;

    public IToken Body => FunctionToken.Body;

    public int Priority => FunctionToken.Priority;

    public bool IsLeftAssociative => FunctionToken.IsLeftAssociative;

    public Function(Scope definedScope, FunctionToken functionToken)
    {
        DefinedScope = definedScope;
        FunctionToken = functionToken;
    }
}