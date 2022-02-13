using Grim.Token;

namespace Grim.VM;

public sealed class Void : IVariable,ExpressionToken
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