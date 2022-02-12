namespace Grim.VM;

public sealed class UnknownVariable : IVariable
{
    public readonly string Name;
    
    public UnknownVariable(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return nameof(UnknownVariable) + $"<{Name}>";
    }
}