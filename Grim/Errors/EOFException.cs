namespace Grim.Errors;

public class EOFException : Exception
{
    public EOFException() : base("プログラムを解析中にプログラムの終端に達しました")
    {
    }
}