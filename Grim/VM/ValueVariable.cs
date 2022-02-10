namespace grim_interpreter.VM;

public class ValueVariable<T> : Variable
{
    public T Value;
    
    public ValueVariable(string varName,T value) : base(varName)
    {
        Value = value;
    }

    public override Variable Copy(string newName)
    {
        return new ValueVariable<T>(newName, Value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}