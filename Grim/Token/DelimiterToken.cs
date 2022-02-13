namespace Grim.Token;

public sealed class DelimiterToken : IToken
{
    public static readonly DelimiterToken Instance = new ();

    private DelimiterToken()
    {
        
    }
    
    public override string ToString()
    {
        return nameof(DelimiterToken);
    }
}