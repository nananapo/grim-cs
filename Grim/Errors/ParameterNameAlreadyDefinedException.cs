namespace Grim.Errors;

public class ParameterNameAlreadyDefinedException : Exception
{
    public readonly string ParamName;
    
    public ParameterNameAlreadyDefinedException(string paramName)
    {
        ParamName = paramName;
    }

    public override string Message => $"{ParamName}は既に定義されています。";
}