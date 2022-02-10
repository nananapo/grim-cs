using Grim.Token;

namespace Grim.VM;

public class VirtualMachine
{

    private readonly RunStack _runStack = new();

    public bool EnableLogging = false;

    private readonly Action<string> _outputFunction;

    private readonly Func<string?> _inputFunction;

    public VirtualMachine(Action<string>? outputFunc = null,Func<string?>? inputFunc = null)
    {
        _outputFunction = outputFunc ?? Console.Write;
        _inputFunction = inputFunc ?? Console.ReadLine;
    }

    private void Debug(string text, int depth)
    {
        if (!EnableLogging) return;
        
        var spaces = "";
        for (int i = 0; i < depth; i++)
            spaces += "    ";
        
        Console.WriteLine(spaces + text);
    }

    public Variable Execute(TermToken seed,Dictionary<string,Variable>? variables = null,int depth = 0)
    {
        _runStack.Push();

        if (variables != null)
        {
            foreach (var (name,variable) in variables)
            {
                _runStack.AssignHere(name,variable);
            }
        }
        
        var exprs = seed.Expressions;
        int index = 0;

        // TODO return判定
        Variable returnVariable = new UnknownVariable(Variable.NoName);
        while(-1 < index && index < exprs.Count)
        {
            Formula formula;
            (index,formula) = NextFormula(seed,index);
            returnVariable = Evaluate(formula,depth+1);
        }
        
        _runStack.Pop();
        return returnVariable;
    }

    private Variable Evaluate(Formula formula,int depth)
    {
        Debug($"Evaluate : {formula}",depth);

        var terms = formula.Terms.ToList();
        var ops = formula.MidOperators.ToList();

        while (terms.Count > 1)
        {
            var max = ops.Select(v => Math.Abs(v.Priority)).Max();
            var lefts = ops.Where(v => v.Priority == -max).ToList();
            var rights = ops.Where(v => v.Priority == max).ToList();

            // すべての符号がそろっていて、右結合の場合
            if (lefts.Count == 0)
            {
                
            }
            // すべてが左結合の場合
            else if (rights.Count == 0)
            {
                
            }
            // 混ざっている
            // 左結合→右結合
            else
            {
                
            }

            break;
        }
        
        var result = Evaluate(terms[0],depth+1);
        Debug($"-> {result}",depth);
        
        return result;
    }

    /// <summary>
    /// TODO 返り値はvoidじゃない
    /// </summary>
    /// <param name="term"></param>
    /// <param name="depth"></param>
    private Variable Evaluate(Term term,int depth)
    {
       Debug($"Evaluate : {term}",depth);
        
        Variable variable;
        
        switch (term.Type)
        {
            case Term.TermType.Term:
                variable = Evaluate(term.MyTerm,depth+1);
                break;
            case Term.TermType.Formula:
                variable = Evaluate(term.Formula,depth+1);
                break;
            case Term.TermType.FunctionCall:
                var funcName = term.FuncCall.Name;
                var formulas = term.FuncCall.Parameters.ToList();
                
                // 関数が見つかった
                if (_runStack.TryGetVariable(funcName,out FunctionToken func))
                {
                    variable = Evaluate(func, formulas,depth+1);
                }
                else
                {
                    // builtin
                    switch (funcName)
                    {
                        case "__let":
                            variable = CallPrimitiveFunction(PrimitiveFunctionType.Let, formulas,depth+1);
                            break;
                        case "__assign":
                            variable = CallPrimitiveFunction(PrimitiveFunctionType.Assign, formulas,depth+1);
                            break;
                        case "__put":
                            variable = CallPrimitiveFunction(PrimitiveFunctionType.Put, formulas,depth+1);
                            break;
                        case "__input":
                            variable = CallPrimitiveFunction(PrimitiveFunctionType.Input, formulas,depth+1);
                            break;
                        default:
                            throw new Exception($"Function {funcName} not found");
                    }
                }
                break;
            case Term.TermType.Variable:
                variable = _runStack.GetVariable(term.Variable.Name);
                // TODO ビルトイン関数の合成は？
                break;
            case Term.TermType.Value:
                variable = term.Value.IsStrValue
                    ? new ValueVariable<string>(Variable.NoName, term.Value.StrValue)
                    : new ValueVariable<int>(Variable.NoName, term.Value.IntValue);
                break;
            case Term.TermType.Function:
                variable = term.Function;
                break;
            default:
                throw new Exception("Unknown error");
        }
        
        // TODO prefixとsuffixを処理
        // TODO 関数なら合成したい
        
        Debug($"-> {variable}",depth);
        return variable;
    }

    private Variable Evaluate(FunctionToken function, List<Formula> parameters,int depth)
    {
        if (function.Parameters.Count != parameters.Count)
            throw new ArgumentException("parameter not match");
        
        // 引数を評価
        var variables = parameters.Select(f=>Evaluate(f,depth)).ToList();

        var dict = new Dictionary<string, Variable>();
        for (int i = 0; i < function.Parameters.Count; i++)
        {
            var pName = function.Parameters[i].Name;
            var variable = variables[i];
            dict[pName] = variable.Copy(pName);//TODO 参照渡し->名前の参照渡し？
        }

        return Execute(function.Body,dict,depth);
    }

    private Variable CallPrimitiveFunction(PrimitiveFunctionType type, List<Formula> parameters,int depth)
    {
        // とりあえず評価しておく
        var variables = parameters.Select(f=>Evaluate(f,depth)).ToList();
        
        switch (type)
        {
            case PrimitiveFunctionType.Assign:
            {
                if (variables.Count != 2)
                    throw new ArgumentException("parameter not match");

                var name = variables[0].VariableName;
                var value = variables[1];
                _runStack.Assign(name, value.Copy(name));
                return value;
            }
            case PrimitiveFunctionType.Put:
            {
                var str = string.Join(" ", variables);
                _outputFunction(str);
                return new ValueVariable<string>(Variable.NoName, str);
            }
            case PrimitiveFunctionType.Input:
            {
                if (variables.Count != 0)
                    throw new ArgumentException("parameter not match");
                
                var str = _inputFunction();
                return new ValueVariable<string>(Variable.NoName, str ?? "");
            }
            default:
                throw new NotImplementedException();
        }
    }

    private (int index,Formula formula) NextFormula(TermToken seed,int index)
    {
        var exprs = seed.Expressions;

        List<Term> terms = new ();
        List<FunctionToken> midOperators = new ();

        while(-1 < index && index < exprs.Count)
        {
            Term term;
            (index, term) = NextTerm(seed, index);
            terms.Add(term);
            
            bool isMidOperator;
            FunctionToken midOp;
            (isMidOperator,index,midOp) = NextMidOperator(seed,index);

            if(!isMidOperator)
            {
                return (index,new Formula(terms,midOperators));
            }

            midOperators.Add(midOp);
        }

        return (-1,new Formula(terms,midOperators));
    }

    private (bool isMidOperator,int index,FunctionToken midOperator) NextMidOperator(TermToken term,int index)
    {
        if(index < 0 || 
           index >= term.Expressions.Count ||
           term.Expressions[index] is not VariableToken variable)
            return (false,index,null)!;

        var searchResult = _runStack.GetVariable(variable.Name);
        
        if(searchResult is not FunctionToken function ||
           function.Type != FunctionType.Mid)
            return (false,index,null)!;
        
        return (true,index+1,function);
    }

    private (int index,Term term) NextTerm(TermToken term,int index)
    {
        var exprs = term.Expressions;

        List<FunctionToken> prefixFuncs;
        Term midTerm;
        List<FunctionToken> suffixFuncs;

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
                }
                midTerm = new Term(new FunctionCall(funcCall.Name,parameters));
                break;
            }
            case FunctionToken func:
            {
                midTerm = new Term(func);
                break;
            }
            case VariableToken variable:
            {
                var name = variable.Name;
                var searchResult = _runStack.GetVariable(name);

                if(searchResult is UnknownVariable)
                {
                    if( name == "__assign" ||
                        name == "__input" ||
                        name == "__put")
                    {
                        midTerm = new Term(new FunctionCall(name));
                        break;
                    }
                    
                    if(int.TryParse(name,out int value))
                    {
                        midTerm = new Term(new ValueToken(value));
                        break;
                    }

                    midTerm = new Term(new VariableToken(name));
                    break;
                }

                if(searchResult is FunctionToken func)
                {
                    if(func.Parameters.Count != 0)
                        throw new Exception($"Function {name} has {func.Parameters.Count} parameters, but no parameter was given.");
                    
                    midTerm = new Term(new FunctionCall(name));
                    break;
                }

                midTerm = new Term(new VariableToken(name));
                break;
            }
            default:
                throw new Exception("なにこれ????" + exprs[index]);
        }

        (index,suffixFuncs) = ReadFixFunctions(term,index+1,true);

        return (index,new Term(prefixFuncs,midTerm,suffixFuncs));
    }

    private (int index, List<FunctionToken> functions) ReadFixFunctions(TermToken term,int index,bool isPrefixMode)
    {
        var functions = new List<FunctionToken>();
        
        var exprs = term.Expressions;
        while(index < exprs.Count)
        {
            var expr = exprs[index];
            if(expr is not VariableToken variable)
            {
                return (index,functions);
            }

            var searchResult = _runStack.GetVariable(variable.Name);

            if(searchResult is not FunctionToken func)
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
}