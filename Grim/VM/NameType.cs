namespace Grim.VM;

public sealed class NameType : IVariable
{
    public readonly string Name;
    
    public NameType(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return typeof(NameType) + $"<{Name}>";
    }
}