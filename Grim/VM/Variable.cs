namespace grim_interpreter.VM;

public abstract class Variable
{
    public const string NoName = "__NONAME__";
    
    public readonly string VariableName;
    
    public Variable(string varName)
    {
        VariableName = varName;
    }

    public abstract Variable Copy(string newName);
}