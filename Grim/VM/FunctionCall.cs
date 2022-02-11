namespace Grim.VM;

public class FunctionCall : IFormula
{
    public readonly string Name;

    public readonly List<IFormula> Parameters;

    public FunctionCall(string name,List<IFormula>? parameters = null)
    {
        Name = name;
        Parameters = parameters ?? new ();
    }

    public override string ToString()
    {
        return nameof(FunctionCall) + $"<{Name}><{string.Join(",",Parameters.Select(v=>v.ToString()))}>";
    }
}