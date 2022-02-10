using Grim.Token;

namespace Grim.VM;

public class Formula : IFormula
{
    public readonly List<Term> Terms;

    public readonly List<FunctionToken> MidOperators;

    public Formula(List<Term> terms,List<FunctionToken> functions)
    {
        Terms = terms;
        MidOperators = functions;
    }

    public override string ToString()
    {
        return nameof(Formula) + $"<T:{string.Join(",",Terms.Select(v=>v.ToString()))}><MO:{string.Join(",",MidOperators.Select(v=>v.ToString()))}>";
    }
}