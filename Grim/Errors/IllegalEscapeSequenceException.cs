namespace Grim.Errors;

public class IllegalEscapeSequenceException : Exception
{
    public readonly string EscapeSequence;
    
    public IllegalEscapeSequenceException(string escapeSequence)
    {
        EscapeSequence = escapeSequence;
    }

    public override string Message => $"エスケープシーケンス \\\"{EscapeSequence}\" を解釈できませんでした。";
}