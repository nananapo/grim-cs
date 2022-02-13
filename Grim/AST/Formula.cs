namespace Grim.VM;

public class Formula : IFormula
{
    public readonly List<IFormula> Terms;

    public readonly List<Function> MidOperators;

    public Formula(List<IFormula> terms,List<Function> functions)
    {
        Terms = terms;
        MidOperators = functions;
    }

    public override string ToString()
    {
        return nameof(Formula) + $"<T:{string.Join(",",Terms)}><MO:{string.Join(",",MidOperators)}>";
    }
}