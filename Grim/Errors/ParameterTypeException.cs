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

    public override string Message => $"{string.Join("と",Values.Select(v=>v.GetType()))} に{FuncName}を適用することができません。";
}