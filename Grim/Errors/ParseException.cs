namespace Grim.Errors;

public class ParseException : Exception
{
    public ParseException(string message) : base("プログラムをパース中に例外が発生しました。\n詳細:\n" + message)
    {
    }
}