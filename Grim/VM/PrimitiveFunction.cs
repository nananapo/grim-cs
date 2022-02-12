namespace Grim.VM;

public class PrimitiveFunction : IVariable
{
    public enum Type
    {
        Put,
        Input,
        Assign,
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
            default:
                primitiveFunction = null;
                return false;
        }
        return true;
    }
}