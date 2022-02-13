namespace Grim.VM;

public class PrimitiveFunction : IVariable
{
    public enum Type
    {
        Put,
        Input,
        Assign,
        Add,
        Negate,
        Equal
    }

    public readonly Type Function;

    private PrimitiveFunction(Type type)
    {
        Function = type;
    }

    public override string ToString()
    {
        return nameof(PrimitiveFunction) + $"<{Function}>";
    }

    public static PrimitiveFunction Create(Type type)
    {
        return new PrimitiveFunction(type);
    }

    public static bool TryParse(string name, out PrimitiveFunction primitiveFunction)
    {
        switch (name)
        {
            case "__assign":
                primitiveFunction = Create(Type.Assign);
                break;
            case "__put":
                primitiveFunction = Create(Type.Put);
                break;
            case "__input":
                primitiveFunction = Create(Type.Input);
                break;
            case "__add":
                primitiveFunction = Create(Type.Add);
                break;
            case "__negate":
                primitiveFunction = Create(Type.Negate);
                break;
            case "__equal":
                primitiveFunction = Create(Type.Equal);
                break;
            default:
                primitiveFunction = null;
                return false;
        }
        return true;
    }
}