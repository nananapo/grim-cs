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
            spaces += "  ";
        
        Console.WriteLine(depth + spaces + text);
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
            IFormula formula;
            (index,formula) = NextFormula(seed,index);
            returnVariable = Evaluate(formula,depth+1);
        }
        
        _runStack.Pop();
        return returnVariable;
    }

    private Variable Evaluate(IFormula target, int depth)
    {
        depth++;
        return target switch
        {
            Formula formula => Evaluate(formula, depth),
            FunctionCall call => Evaluate(call, depth),
            ValueToken valueToken => Evaluate(valueToken,depth),
            ModifierTerm modifierTerm => Evaluate(modifierTerm,depth),
            FunctionToken functionToken => functionToken,
            Variable variable => variable,
            _ => throw new NotImplementedException(target.GetType().FullName)
        };
    }

    private Variable Evaluate(Formula formula,int depth)
    {
        Debug($"EvaluateF : {formula}",depth);

        Variable result;
        
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
        result = Evaluate(terms[0],depth+1);
        
        Debug($"-> {result}",depth);
        return result;
    }

    private Variable Evaluate(FunctionCall funcCall,int depth)
    {
        Debug($"EvaluateC : {funcCall}",depth);
        
        var funcName = funcCall.Name;
        var formulas = funcCall.Parameters.ToList();
                    
        // 関数が見つかった
        if (_runStack.TryGetVariable(funcName,out FunctionToken function))
        {
            if (function.Parameters.Count != formulas.Count)
                throw new ArgumentException("parameter not match");
        
            // 引数を評価
            var variables = formulas.Select(f=>Evaluate(f,depth)).ToList();

            var dict = new Dictionary<string, Variable>();
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                var pName = function.Parameters[i].Name;
                dict[pName] = variables[i].Copy(pName);//TODO 参照渡し->名前の参照渡し？
            }

            return Execute(function.Body,dict,depth);
        }
                    
        // builtin
        Variable variable;
        switch (funcName)
        {
            case "__let":
                variable = CallPrimitiveFunction(PrimitiveFunctionType.Let, formulas,depth);
                break;
            case "__assign":
                variable = CallPrimitiveFunction(PrimitiveFunctionType.Assign, formulas,depth);
                break;
            case "__put":
                variable = CallPrimitiveFunction(PrimitiveFunctionType.Put, formulas,depth);
                break;
            case "__input":
                variable = CallPrimitiveFunction(PrimitiveFunctionType.Input, formulas,depth);
                break;
            default:
                throw new Exception($"Function {funcName} not found");
        }

        return variable;
    }

    private Variable Evaluate(ValueToken token,int depth)
    {
        Debug($"EvaluateV : {token}",depth);
        return token.IsStrValue
            ? new ValueVariable<string>(Variable.NoName, token.StrValue)
            : new ValueVariable<int>(Variable.NoName, token.IntValue);
    }

    /// <summary>
    /// TODO 返り値はvoidじゃない
    /// </summary>
    /// <param name="term"></param>
    /// <param name="depth"></param>
    private Variable Evaluate(ModifierTerm term,int depth)
    { 
        Debug($"EvaluateM : {term}",depth);

        Variable variable = Evaluate(term.Term,depth+1);
        // TODO prefixとsuffixを処理
        // TODO 関数なら合成
        
        // TODO ビルトイン関数の合成は？
        // TODO Unknownでいいの？ 名前は？

        Debug($"-> {variable}",depth);
        return variable;
    }

    private Variable CallPrimitiveFunction(PrimitiveFunctionType type, List<IFormula> parameters,int depth)
    {        
        Debug($"CallPrmvF : {type}",depth);

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

    private (int index,IFormula formula) NextFormula(TermToken seed,int index)
    {
        var exprs = seed.Expressions;

        List<IFormula> terms = new ();
        List<FunctionToken> midOperators = new ();

        while(-1 < index && index < exprs.Count)
        {
            // Termを1つ読む
            IFormula term;
            (index, term) = NextTerm(seed, index);
            terms.Add(term);
            
            // 中値演算子を1つ読む
            bool isMidOperator;
            FunctionToken midOp;
            (isMidOperator,index,midOp) = NextMidOperator(seed,index);
            
            if(!isMidOperator)
                break;
            
            midOperators.Add(midOp);
        }
        
        // 中値演算子の数が合わないならエラー
        if (terms.Count - 1 != midOperators.Count)
        {
            throw new Exception("中値演算子の数は");
        }

        // Termが一つならそれを返す
        if (midOperators.Count == 0 && terms.Count == 1)
        {
            return (index, terms[0]);
        }
        
        return (index,new Formula(terms,midOperators));
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

    private (int index,IFormula term) NextTerm(TermToken target,int index)
    {
        var exprs = target.Expressions;

        List<FunctionToken> prefixFuncs;
        IFormula midTerm;
        List<FunctionToken> suffixFuncs;

        (index,prefixFuncs) = ReadFixFunctions(target,index,true);

        if(index == -1 && prefixFuncs.Count != 0)
        {
            throw new Exception($"There are {prefixFuncs.Count} prefix operators, but formula is not found.");
        }

        switch(exprs[index])
        {
            case ValueToken value:
            {
                midTerm = value;
                break;
            }
            case TermToken term:
            {
                // TODO 読むのは一つだけ？
                IFormula formula;
                (_,formula) = NextFormula(term,0);
                midTerm = formula;
                break;
            }
            case FunctionCallToken funcCall:
            {
                int nindex = 0;
                List<IFormula> parameters = new();
                while(-1 < nindex && nindex < funcCall.Parameters.Expressions.Count)
                {
                    IFormula formula;
                    (nindex,formula) = NextFormula(funcCall.Parameters,nindex);
                    parameters.Add(formula);
                }
                midTerm = new FunctionCall(funcCall.Name,parameters);
                break;
            }
            case FunctionToken func:
            {
                midTerm = func;
                break;
            }
            case VariableToken variable:
            {
                var name = variable.Name;
                var searchResult = _runStack.GetVariable(name);

                if(searchResult is UnknownVariable unknown)
                {
                    if( name == "__assign" ||
                        name == "__input" ||
                        name == "__put")
                    {
                        midTerm = new FunctionCall(name);
                        break;
                    }
                    
                    if(int.TryParse(name,out int value))
                    {
                        midTerm = new ValueToken(value);
                        break;
                    }

                    midTerm = unknown;
                    break;
                }

                if(searchResult is FunctionToken func)
                {
                    if(func.Parameters.Count != 0)
                        throw new Exception($"Function {name} has {func.Parameters.Count} parameters, but no parameter was given.");
                    
                    midTerm = new FunctionCall(name);
                    break;
                }

                midTerm = searchResult;
                break;
            }
            default:
                throw new Exception("なにこれ????" + exprs[index]);
        }

        (index,suffixFuncs) = ReadFixFunctions(target,index+1,true);

        // 前置演算子も後置演算子もないならそのまま返す
        if (prefixFuncs.Count == 0 &&
            suffixFuncs.Count == 0)
        {
            return (index, midTerm);
        }
        
        // 演算子で修飾して返す
        return (index,new ModifierTerm(prefixFuncs,midTerm,suffixFuncs));
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