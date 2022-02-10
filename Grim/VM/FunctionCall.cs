namespace Grim.VM;

public class FunctionCall
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
        return nameof(FunctionCall) + $"<{Name}><P:{string.Join(",",Parameters.Select(v=>v.ToString()))}>";
    }
}