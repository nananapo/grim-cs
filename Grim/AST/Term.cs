using Grim.VM;

namespace Grim.AST;

public class Term : IFormula
{
    public readonly IFormula MidFormula;
    public readonly List<Function> PrefixFuncs;
    public readonly List<Function> SuffixFuncs;
    
    public Term(List<Function> prefix,IFormula midFormula,List<Function> suffix)
    {
        PrefixFuncs = prefix;
        SuffixFuncs = suffix;
        MidFormula = midFormula;
    }
    
    public override string ToString()
    {
        return $"MTerm<P:{string.Join(",",PrefixFuncs)}>" +
               $"<T:{MidFormula}>" +
               $"<S:{string.Join(",",SuffixFuncs)}>";
    }
}