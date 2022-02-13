namespace Grim.VM;

public sealed class Void : IVariable
{
    private Void()
    {
        
    }

    public static readonly Void Instance = new();

    public override string ToString()
    {
        return nameof(Void);
    }
}