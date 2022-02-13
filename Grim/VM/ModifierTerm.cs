using Grim.Token;

namespace Grim.VM;

public class ModifierTerm : IFormula
{
    public readonly IFormula Term;
    public readonly List<Function> PrefixFuncs;
    public readonly List<Function> SuffixFuncs;
    
    public ModifierTerm(List<Function> prefix,IFormula term,List<Function> suffix)
    {
        PrefixFuncs = prefix;
        SuffixFuncs = suffix;
        Term = term;
    }
    
    public override string ToString()
    {
        return $"MTerm<P:{string.Join(",",PrefixFuncs)}>" +
               $"<T:{Term}>" +
               $"<S:{string.Join(",",SuffixFuncs)}>";
    }
}