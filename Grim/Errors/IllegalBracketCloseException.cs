namespace Grim.Errors;

public class IllegalBracketCloseException : Exception
{
    public IllegalBracketCloseException() : base("閉じ括弧の数が多すぎます。")
    {
    }
}