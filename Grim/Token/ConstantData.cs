using Grim.VM;

namespace Grim.Token;

public class ConstantData<T> : IVariable,IToken,IEquatable<ConstantData<T>>
{
    public T Value;
    
    public ConstantData(T value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public bool Equals(ConstantData<T>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ConstantData<T>) obj);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode(Value);
    }
}