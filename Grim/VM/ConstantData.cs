using Grim.Token;

namespace Grim.VM;

public class ConstantData<T> : IVariable,ExpressionToken
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