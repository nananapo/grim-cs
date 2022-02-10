using grim_interpreter.Token;

namespace grim_interpreter.VM;

public class Function : Variable
{
    public readonly FunctionType Type;

    public readonly List<VariableToken> Parameters;

    public readonly List<Formula> Formulas;

    public readonly int Priority;

    public Function(string varName,FunctionType type,int priority,List<VariableToken> parameters,List<Formula> formulas) : base(varName)
    {
        Type = type;
        Priority = priority;
        Parameters = parameters;
        Formulas = formulas;
    }

    public override Function Copy(string newName)
    {
        return new Function(newName,Type,Priority,Parameters.ToList(),Formulas.ToList());
    }
}