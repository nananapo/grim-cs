namespace Grim.VM;

public sealed class NameType : IVariable,IEquatable<NameType>
{
    
    public readonly string Name;

    public readonly int DefinedScopeId;
    
    public NameType(string name,int definedScopeId)
    {
        Name = name;
        DefinedScopeId = definedScopeId;
    }

    public override string ToString()
    {
        return nameof(NameType) + $"<{Name}>";
    }

    public bool Equals(NameType? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && DefinedScopeId.Equals(other.DefinedScopeId);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is NameType other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, DefinedScopeId);
    }
}