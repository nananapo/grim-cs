namespace Grim.Errors;

public class ParameterTypeException : Exception
{
    private readonly string FuncName;
    private readonly object[] Values;
    
    public ParameterTypeException(string funcName,params object[] values)
    {
        FuncName = funcName;
        Values = values;
    }

    public override string Message => $"{FuncName}に{string.Join("と",Values.Select(v=>v.GetType()))}を適用することができません。";
}