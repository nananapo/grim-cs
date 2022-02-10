namespace Grim.Token;

public class ValueToken : ExpressionToken
{
    public readonly bool IsStrValue;

    public readonly string StrValue;

    public readonly int IntValue;

    public ValueToken(string value)
    {
        IsStrValue = true;
        StrValue = value;
    }

    public ValueToken(int value)
    {
        IsStrValue = false;
        IntValue = value;
    }

    public override string ToString()
    {
        return nameof(ValueToken) + (IsStrValue ? $"<String><{StrValue}>" : $"<Int><{IntValue}>");
    }
}