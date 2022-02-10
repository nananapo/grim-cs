namespace grim_interpreter.Token;

public class VariableToken : ExpressionToken
{

    public readonly string Name;

    public VariableToken(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return nameof(VariableToken) + "<" + Name + ">";
    }
}