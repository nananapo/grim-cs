public class VirtualMachine
{

    private readonly VirtualMachine Parent;

    private readonly Dictionary<string,object> Variables = new (); 

    public VirtualMachine(VirtualMachine parent = null)
    {
        Parent = parent;
    }

    public void Execute(TermToken seed)
    {
        var exprs = seed.Expressions;
        int index = 0;

        while(-1 < index && index < exprs.Count)
        {
            Formula formula;
            (index,formula) = NextFormula(seed,index);
            
            Console.WriteLine($"Formula[{index}]--\n{formula}");
        }
    }

    public void Evaluate(Formula formula)
    {
        
    }

    private (int index,Formula formula) NextFormula(TermToken seed,int index)
    {
        var exprs = seed.Expressions;

        List<Term> terms = new ();
        List<Function> midOperators = new ();

        while(-1 < index && index < exprs.Count)
        {
            Term term;
            (index, term) = NextTerm(seed, index);
            terms.Add(term);
            
            bool isMidOperator;
            Function midOp;
            (isMidOperator,index,midOp) = NextMidOperator(seed,index);

            if(!isMidOperator)
            {
                return (index,new Formula(terms,midOperators));
            }

            midOperators.Add(midOp);
        }

        return (-1,new Formula(terms,midOperators));
    }

    private (bool isMidOperator,int index,Function midOperator) NextMidOperator(TermToken term,int index)
    {
        var exprs = term.Expressions;
    
        if(index < 0 || index >= exprs.Count)
            return (false,index,null);

        var expr = exprs[index];
        if(expr is not VariableToken variable)
            return (false,index,null);

        var searchResult = GetVariable(variable.Name);
        if(searchResult == null)
            return (false,index,null);
        
        if(searchResult is not Function function)
            return (false,index,null);
        
        if(function.Type != FunctionType.Mid)
            return (false,index,null);
        
        return (true,index+1,function);
    }

    private (int index,Term term) NextTerm(TermToken term,int index)
    {
        var exprs = term.Expressions;

        List<Function> prefixFuncs;
        Term midTerm;
        List<Function> suffixFuncs;

        (index,prefixFuncs) = ReadFixFunctions(term,index,true);

        if(index == -1 && prefixFuncs.Count != 0)
        {
            throw new Exception($"There are {prefixFuncs.Count} prefix operators, but formula is not found.");
        }

        switch(exprs[index])
        {
            case ValueToken value:
            {
                midTerm = new Term(value);
                break;
            }
            case TermToken nterm:
            {
                Formula formula;
                (_,formula) = NextFormula(nterm,0);
                midTerm = new Term(formula);
                break;
            }
            case FunctionCallToken funcCall:
            {
                int nindex = 0;
                List<Formula> parameters = new();
                while(-1 < nindex && nindex < funcCall.Parameters.Expressions.Count)
                {
                    Formula formula;
                    (nindex,formula) = NextFormula(funcCall.Parameters,nindex);
                    parameters.Add(formula);
                    //Console.WriteLine(nindex + " " + formula);
                }
                midTerm = new Term(new FunctionCall(funcCall.Name,parameters));
                break;
            }
            case VariableToken variable:
            {
                var name = variable.Name;
                var searchResult = GetVariable(name);

                if(searchResult == null)
                {
                    if( name == "__assign" ||
                        name == "__input" ||
                        name == "__put")
                    {
                        midTerm = new Term(new FunctionCall(name,null));
                        break;
                    }
                    
                    if(int.TryParse(name,out int value))
                    {
                        midTerm = new Term(new ValueToken(value));
                        break;
                    }

                    //throw new Exception($"Unknown variable {name}");
                    midTerm = new Term(new VariableToken(name));
                    break;
                }

                if(searchResult is Function func)
                {
                    if(func.Parameters.Count != 0)
                        throw new Exception($"Function {name} has {func.Parameters.Count} parameters, but no parameter was given.");
                    
                    midTerm = new Term(new FunctionCall(name,null));
                    break;
                }

                midTerm = new Term(new VariableToken(name));
                break;
            }
            default:
                throw new Exception("nanikore ????");
        }

        (index,suffixFuncs) = ReadFixFunctions(term,index+1,true);

        return (index,new Term(prefixFuncs,midTerm,suffixFuncs));
    }

    public (int index, List<Function> functions) ReadFixFunctions(TermToken term,int index,bool isPrefixMode)
    {
        var functions = new List<Function>();
        
        var exprs = term.Expressions;
        while(index < exprs.Count)
        {
            var expr = exprs[index];
            if(expr is not VariableToken variable)
            {
                return (index,functions);
            }

            var searchResult = GetVariable(variable.Name);
            if(searchResult == null)
            {
                return (index,functions);
            }

            if(searchResult is not Function func)
            {
                return (index,functions);
            }

            
            if(!(func.Type == FunctionType.Prefix && isPrefixMode) 
            && !(func.Type == FunctionType.Suffix && !isPrefixMode))
            {
                return (index,functions);
            }

            functions.Add(func);
            index++;
        }

        return (-1,functions);
    }

    public object GetVariable(string name)
    {
        return Variables.ContainsKey(name) ? Variables[name] : (Parent != null ? Parent.GetVariable(name) : null);
    }

    public void SetVariable(string name,object value)
    {
        Variables[name] = value;
    }
}