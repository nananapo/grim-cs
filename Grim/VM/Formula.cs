namespace grim_interpreter.VM;

public class Formula
{
    public readonly List<Term> Terms;

    public readonly List<Function> MidOperators;

    public Formula(List<Term> terms,List<Function> functions)
    {
        Terms = terms;
        MidOperators = functions;
    }

    public override string ToString()
    {
        return nameof(Formula) + $"<T:{string.Join(",",Terms.Select(v=>v.ToString()))}><MO:{string.Join(",",MidOperators.Select(v=>v.ToString()))}>";
    }
}