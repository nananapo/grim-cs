namespace Grim.VM;

public class FunctionCall : IFormula
{

    public readonly IFormula Lambda;

    public readonly List<IFormula> Parameters;

    public FunctionCall(IFormula lambda,List<IFormula> parameters)
    {
        Lambda = lambda;
        Parameters = parameters;
    }

    public override string ToString()
    {
        return nameof(FunctionCall) + $"<{Lambda}><{string.Join(",",Parameters.Select(v=>v.ToString()))}>";
    }
}