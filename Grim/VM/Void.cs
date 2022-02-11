using Grim.Token;

namespace Grim.VM;

public sealed class Void : IVariable,ExpressionToken
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