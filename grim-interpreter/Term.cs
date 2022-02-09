using System.Linq;
using System.Collections.Generic;

public class Term
{

    public List<Function> PrefixFuncs;
    public List<Function> SuffixFuncs;

    public enum TermType
    {
        Term,
        Formula,
        Variable,
        Value,
        FunctionCall
        //TODO Function
    }

    public readonly TermType Type;

    public Term MyTerm;
    public Formula Formula;
    public VariableToken Variable;
    public ValueToken Value;
    public FunctionCall FuncCall;


    public Term(Term term)
    {
        Type = TermType.Term;
        PrefixFuncs = new List<Function>();
        SuffixFuncs = new List<Function>();
        MyTerm = term;
    }

    public Term(List<Function> prefix,Term term,List<Function> suffix)
    {
        Type = TermType.Term;
        PrefixFuncs = prefix;
        SuffixFuncs = suffix;
        MyTerm = term;
    }

    public Term(Formula formula)
    {
        Type = TermType.Formula;
        PrefixFuncs = new List<Function>();
        SuffixFuncs = new List<Function>();
        Formula = formula;
    }

    public Term(VariableToken variable)
    {
        Type = TermType.Variable;
        PrefixFuncs = new List<Function>();
        SuffixFuncs = new List<Function>();
        Variable = variable;
    }

    public Term(ValueToken value)
    {
        Type = TermType.Value;
        PrefixFuncs = new List<Function>();
        SuffixFuncs = new List<Function>();
        Value = value;
    }

    public Term(FunctionCall call)
    {
        Type = TermType.FunctionCall;
        PrefixFuncs = new List<Function>();
        SuffixFuncs = new List<Function>();
        FuncCall = call;
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
        }
        str += $"><S:{string.Join(",",SuffixFuncs.Select(v=>v.ToString()))}>";
        return str;
    }
}