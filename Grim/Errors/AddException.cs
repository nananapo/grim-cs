namespace Grim.Errors;

public class AddException : Exception
{
    private readonly object Value1;
    private readonly object Value2;
    
    public AddException(object va1, object va2)
    {
        Value1 = va1;
        Value2 = va2;
    }

    public override string Message => $"{Value1.GetType()} と {Value2.GetType()} にAddを適用することができません。";
}