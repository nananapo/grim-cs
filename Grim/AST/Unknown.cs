namespace Grim.AST;

public sealed class Unknown : IFormula
{
    public readonly string Name;
    
    public Unknown(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return nameof(Unknown) + $"<{Name}>";
    }
}