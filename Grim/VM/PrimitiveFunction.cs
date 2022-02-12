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
}