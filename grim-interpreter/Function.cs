using System.Collections.Generic;

public class Function
{
    public readonly FunctionType Type;

    public readonly List<VariableToken> Parameters;

    public readonly List<Formula> Formulas;

    public Function(FunctionType type,List<VariableToken> parameters,List<Formula> formulas)
    {
        Type = type;
        Parameters = parameters;
        Formulas = formulas;
    }
}