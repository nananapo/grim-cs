public class FunctionCall
{
    public readonly string Name;

    public readonly List<Formula> Parameters;

    public FunctionCall(string name,List<Formula> parameters)
    {
        Name = name;
        Parameters = parameters ?? new ();
    }

    public override string ToString()
    {
        return nameof(FunctionCall) + $"<{Name}><P:{string.Join(",",Parameters.Select(v=>v.ToString()))}>";
    }
}