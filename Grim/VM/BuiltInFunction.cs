namespace Grim.VM;

public  class BuiltInFunction : IVariable
{
    public readonly BuiltInFunctionType Function;

    private BuiltInFunction(BuiltInFunctionType type)
    {
        Function = type;
    }

    public static BuiltInFunction Create(BuiltInFunctionType type)
    {
        return new BuiltInFunction(type);
    }

    public override string ToString()
    {
        return nameof(BuiltInFunction) + $"<{Function}>";
    }

    public static bool TryParse(string name, out BuiltInFunction builtInFunction)
    {
        switch (name)
        {
            case "__assign":
                builtInFunction = Create(BuiltInFunctionType.Assign);
                break;
            case "__put":
                builtInFunction = Create(BuiltInFunctionType.Put);
                break;
            case "__input":
                builtInFunction = Create(BuiltInFunctionType.Input);
                break;
            case "__add":
                builtInFunction = Create(BuiltInFunctionType.Add);
                break;
            case "__negate":
                builtInFunction = Create(BuiltInFunctionType.Negate);
                break;
            case "__equal":
                builtInFunction = Create(BuiltInFunctionType.Equal);
                break;
            default:
                builtInFunction = null;
                return false;
        }
        return true;
    }
}