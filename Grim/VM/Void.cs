namespace Grim.VM;

public sealed class Void : IVariable
{
    private Void()
    {
        
    }

    public static Void Create() => new ();

    public override string ToString()
    {
        return nameof(Void);
    }
}