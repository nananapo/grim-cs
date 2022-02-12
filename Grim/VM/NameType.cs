namespace Grim.VM;

public sealed class NameType : IVariable
{

    //TODO 定義場所
    
    public readonly string Name;
    
    public NameType(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return nameof(NameType) + $"<{Name}>";
    }
}