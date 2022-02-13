namespace Grim.Token;

public class VariableToken : IToken
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