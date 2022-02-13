namespace Grim.VM;

public sealed class NameType : IVariable,IEquatable<NameType>
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

    public bool Equals(NameType? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && Scope.Equals(other.Scope);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is NameType other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Scope);
    }
}