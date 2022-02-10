using Grim.Token;

namespace Grim.VM;

public class NonModifierTerm : Term
{
    public readonly TermType Type;

    public readonly Formula Formula;
    public readonly VariableToken Variable;
    public readonly ValueToken Value;
    public readonly FunctionCall FuncCall;
    public readonly FunctionToken Function;
    
    public NonModifierTerm(Formula formula)
    {
        Type = TermType.Formula;
        Formula = formula;
    }

    public NonModifierTerm(VariableToken variable)
    {
        Type = TermType.Variable;
        Variable = variable;
    }

    public NonModifierTerm(ValueToken value)
    {
        Type = TermType.Value;
        Value = value;
    }

    public NonModifierTerm(FunctionCall call)
    {
        Type = TermType.FunctionCall;
        FuncCall = call;
    }

    public NonModifierTerm(FunctionToken func)
    {
        Type = TermType.Function;
        Function = func;
    }
    
    public override string ToString()
    {
        string str = Type switch
        {
            TermType.Formula => Formula.ToString(),
            TermType.Variable => Variable.ToString(),
            TermType.Value => Value.ToString(),
            TermType.FunctionCall => FuncCall.ToString(),
            TermType.Function => Function.ToString(),
            _ => throw new NotImplementedException()
        };
        return nameof(Term) + $"<{Type}><{str}>";
    }
}