using Grim.Token;

namespace Grim.VM;

public class ModifierTerm : IFormula
{
    public readonly IFormula Term;
    public readonly List<FunctionToken> PrefixFuncs;
    public readonly List<FunctionToken> SuffixFuncs;
    
    public ModifierTerm(List<FunctionToken> prefix,IFormula term,List<FunctionToken> suffix)
    {
        PrefixFuncs = prefix;
        SuffixFuncs = suffix;
        Term = term;
    }
    
    public override string ToString()
    {
        return $"MTerm<P:{string.Join(",",PrefixFuncs.Select(v=>v.ToString()))}>" +
               $"<T:{Term}>" +
               $"<S:{string.Join(",",SuffixFuncs.Select(v=>v.ToString()))}>";
    }
}