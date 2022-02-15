using Grim.Token;

namespace Grim.VM;

public class Function : IVariable
{
    public readonly int DefinedScopeId;

    public readonly FunctionToken FunctionToken;

    public FunctionType Type => FunctionToken.Type;

    public IList<string> Parameters => FunctionToken.Parameters;

    public List<IToken> Body => FunctionToken.Body;

    public int Priority => FunctionToken.Priority;

    public bool IsLeftAssociative => FunctionToken.IsLeftAssociative;

    public BuiltInFunctionType BuiltInFunctionType => FunctionToken.BuiltInFunctionType;

    // 既に適用されたパラメーター
    public List<IVariable> AppliedParameterVariables = new ();

    public Function(int definedScopeId, FunctionToken functionToken)
    {
        DefinedScopeId = definedScopeId;
        FunctionToken = functionToken;
    }

    public Function(BuiltInFunctionType builtInFunctionType)
    {
        DefinedScopeId = -1;
        FunctionToken = new FunctionToken(builtInFunctionType);
    } 
    
}