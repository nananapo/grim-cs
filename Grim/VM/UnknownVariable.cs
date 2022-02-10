namespace Grim.VM;

public class UnknownVariable : Variable
{
    public UnknownVariable(string varName) : base(varName)
    {
        
    }

    public override UnknownVariable Copy(string newName)
    {
        return new UnknownVariable(newName);
    }

    public override string ToString()
    {
        return nameof(UnknownVariable) + "<" + VariableName + ">";
    }
}