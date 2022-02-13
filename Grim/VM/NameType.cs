namespace Grim.VM;

public sealed class NameType : IVariable,IFormula
{
    
    public readonly string Name;

    public readonly Scope Scope;
    
    public NameType(string name,Scope scope)
    {
        Name = name;
        Scope = scope;
    }

    public override string ToString()
    {
        return nameof(NameType) + $"<{Name}>";
    }
}