namespace Grim.Token;

public enum BuiltInFunctionType
{
    Put,
    PERROR,
    Input,
    Assign,
    Add,
    Negate,
    Equal,
    If,
    IfElse,
    While,
    ReadFile,
    StrAt,
    Eval
}

public static class BuiltInFunctionHelper
{
    public static bool TryParse(string name, out BuiltInFunctionType type)
    {
        switch (name)
        {
            case "__assign":
                type = BuiltInFunctionType.Assign;
                break;
            case "__put":
                type = BuiltInFunctionType.Put;
                break;
            case "__perror":
                type = BuiltInFunctionType.PERROR;
                break;
            case "__input":
                type = BuiltInFunctionType.Input;
                break;
            case "__add":
                type = BuiltInFunctionType.Add;
                break;
            case "__negate":
                type = BuiltInFunctionType.Negate;
                break;
            case "__equal":
                type = BuiltInFunctionType.Equal;
                break;
            case "__if":
                type = BuiltInFunctionType.If;
                break;
            case "__ifElse":
                type = BuiltInFunctionType.IfElse;
                break;
            case "__while":
                type = BuiltInFunctionType.While;
                break;
            case "__read":
                type = BuiltInFunctionType.ReadFile;
                break;
            case "__strAt":
                type = BuiltInFunctionType.StrAt;
                break;
            case "__eval":
                type = BuiltInFunctionType.Eval;
                break;
            default:
                type = BuiltInFunctionType.Assign;
                return false;
        }
        return true;
    }

    public static readonly Dictionary<BuiltInFunctionType, int> BuiltInFunctionParameterCounts = new()
    {
        {BuiltInFunctionType.Assign,2},
        {BuiltInFunctionType.Put,1},
        {BuiltInFunctionType.PERROR,1},
        {BuiltInFunctionType.Input,0},
        {BuiltInFunctionType.Add,2},
        {BuiltInFunctionType.Negate,1},
        {BuiltInFunctionType.Equal,2},
        {BuiltInFunctionType.If,2},
        {BuiltInFunctionType.IfElse,3},
        {BuiltInFunctionType.While,2},
        {BuiltInFunctionType.ReadFile,1},
        {BuiltInFunctionType.StrAt,2},
        {BuiltInFunctionType.Eval,1}
    };
}