using Grim.Token;

namespace Grim.VM;

public class Formula : IFormula
{
    public readonly List<IFormula> Terms;

    public readonly List<FunctionToken> MidOperators;

    public Formula(List<IFormula> terms,List<FunctionToken> functions)
    {
        Terms = terms;
        MidOperators = functions;
    }

    public override string ToString()
    {
        return nameof(Formula) + $"<T:{string.Join(",",Terms)}><MO:{string.Join(",",MidOperators)}>";
    }
}