namespace Grim.Errors;

public class IllegalParameterNameException : Exception
{
    public readonly string ParamName;
    
    public IllegalParameterNameException(string paramName)
    {
        ParamName = paramName;
    }

    public override string Message => $"\"{ParamName}\"をパラメーター名として使うことはできません。";
}