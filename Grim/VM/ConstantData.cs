namespace Grim.VM;

public class ConstantData<T> : IVariable
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
}