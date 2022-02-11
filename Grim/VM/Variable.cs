namespace Grim.VM;

public abstract class Variable : IFormula
{
    public const string NoName = "__NONAME__";
    
    public readonly string VariableName;
    
    public Variable(string varName)
    {
        VariableName = varName;
    }

    public abstract Variable Copy(string newName);
}