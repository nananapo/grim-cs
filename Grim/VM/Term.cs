using Grim.Token;

namespace Grim.VM;

public class Term
{

    public readonly List<FunctionToken> PrefixFuncs;
    public readonly List<FunctionToken> SuffixFuncs;

    public enum TermType
    {
        Term,
        Formula,
        Variable,
        Value,
        FunctionCall,
        Function
    }

    public readonly TermType Type;

    public readonly Term MyTerm;
    public readonly Formula Formula;
    public readonly VariableToken Variable;
    public readonly ValueToken Value;
    public readonly FunctionCall FuncCall;
    public readonly FunctionToken Function;

    public Term(List<FunctionToken> prefix,Term term,List<FunctionToken> suffix)
    {
        Type = TermType.Term;
        PrefixFuncs = prefix;
        SuffixFuncs = suffix;
        MyTerm = term;
    }

    public Term(Formula formula)
    {
        Type = TermType.Formula;
        PrefixFuncs = new List<FunctionToken>();
        SuffixFuncs = new List<FunctionToken>();
        Formula = formula;
    }

    public Term(VariableToken variable)
    {
        Type = TermType.Variable;
        PrefixFuncs = new List<FunctionToken>();
        SuffixFuncs = new List<FunctionToken>();
        Variable = variable;
    }

    public Term(ValueToken value)
    {
        Type = TermType.Value;
        PrefixFuncs = new List<FunctionToken>();
        SuffixFuncs = new List<FunctionToken>();
        Value = value;
    }

    public Term(FunctionCall call)
    {
        Type = TermType.FunctionCall;
        PrefixFuncs = new List<FunctionToken>();
        SuffixFuncs = new List<FunctionToken>();
        FuncCall = call;
    }

    public Term(FunctionToken func)
    {
        Type = TermType.Function;
        PrefixFuncs = new List<FunctionToken>();
        SuffixFuncs = new List<FunctionToken>();
        Function = func;
    }

    public override string ToString()
    {
        var str = nameof(Term) + $"<{Type}><P:{string.Join(",",PrefixFuncs.Select(v=>v.ToString()))}><T:";
        switch(Type)
        {
            case TermType.Term:
                str += MyTerm.ToString();
                break;
            case TermType.Formula:
                str += Formula.ToString();
                break;
            case TermType.Variable:
                str += Variable.ToString();
                break;
            case TermType.Value:
                str += Value.ToString();
                break;
            case TermType.FunctionCall:
                str += FuncCall.ToString();
                break;
            case TermType.Function:
                str += Function.ToString();
                break;
        }
        str += $"><S:{string.Join(",",SuffixFuncs.Select(v=>v.ToString()))}>";
        return str;
    }
}