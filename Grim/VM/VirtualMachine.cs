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
        
        var spaces = " ";
        for (int i = 0; i < depth; i++)
            spaces += "  ";
        
        Console.WriteLine(depth + spaces + text);
    }

    public IVariable Execute(ExpressionToken seed,Dictionary<string,IVariable>? variables = null,int depth = 0)
    {
        _runStack.Push();

        // 引数を環境に入れる
        // TODO 静的スコープ
        if (variables != null)
        {
            foreach (var (name,variable) in variables)
            {
                _runStack.AssignHere(name,variable);
            }
        }

        var formula = ReadFormula(seed);
        //Console.WriteLine($"FORMULA : {formula}");
        var result = Evaluate(formula,depth+1);
        
        _runStack.Pop();
        return result;
    }

    private IVariable Evaluate(IFormula target, int depth)
    {
        //Debug($"EA {target}",depth);

        if (target is UnknownVariable unknown)
        {
            var result =_runStack.GetVariable(unknown.Name);
            if (result is Void)
            {
                // 不明ならエラー
                throw new Exception($"\"{unknown.Name}\"を解決できませんでした");
            }
            return result;
        }
        
        return target switch
        {
            // 定数
            ConstantData<string> value => value,
            ConstantData<int> value => value,
            PrimitiveFunction func => func,
            
            Formula formula => Evaluate(formula, depth),
            FunctionCall call => Evaluate(call, depth),
            ModifierTerm modifierTerm => EvaluateTerm(modifierTerm,depth),
            FunctionToken functionToken => functionToken,
            NameType nameType => nameType,
            Void v => v,
            _ => throw new NotImplementedException(target.GetType().FullName)
        };
    }

    private IVariable Evaluate(Formula formula,int depth)
    {
        Debug($"EvalF : {formula}",depth);
        
        IVariable result;
        
        // TODO これはParse段階で除外したい
        if (formula.Terms.Count == 0)
        {
            if (formula.MidOperators.Count != 0)
                throw new Exception("中値演算子が不正な位置にあります");

            result = Void.Create();
            Debug($"-> {result}",depth);
            return result;
        }
        
        var terms = formula.Terms.ToList();
        var ops = formula.MidOperators.ToList();
        while (terms.Count > 1)
        {
            // 式だけが連続する場合、最後のTermを残して抜ける
            if (ops.Count == 0)
            {
                while(terms.Count > 1)
                {
                    Evaluate(terms[0], depth + 1);
                    terms.RemoveAt(0);
                }
                break;
            }

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

    private IVariable Evaluate(FunctionCall funcCall,int depth)
    {
        Debug($"EvalC : {funcCall}",depth);
        // 関数を取得する
        var function = Evaluate(funcCall.Lambda, depth+1);

        IVariable result;
        
        // 正常に取得できた場合、実行する
        if (function is FunctionToken functionToken)
        {
            result = CallFunction(functionToken,funcCall.Parameters,depth+1);
        }
        // builtin
        else if (function is PrimitiveFunction primitive)
        {
            result = EvaluatePrimitiveFunction(primitive, funcCall.Parameters, depth + 1);
        }
        // それ以外ならエラー
        else
        {
            throw new  Exception("Value is not function");
        }

        Debug($"-> {result}",depth);
        return result;
    }

    /// <summary>
    /// </summary>
    /// <param name="term"></param>
    /// <param name="depth"></param>
    private IVariable EvaluateTerm(ModifierTerm term,int depth)
    { 
        Debug($"EvalMT : {term}",depth);

        IVariable variable = Evaluate(term.Term,depth+1);
        
        
        
        // TODO prefixとsuffixを処理
        // TODO 関数なら合成
        
        // TODO ビルトイン関数の合成は？
        // TODO Unknownでいいの？ 名前は？

        Debug($"-> {variable}",depth);
        return variable;
    }

    /// <summary>
    /// FunctionTokenを評価する
    /// </summary>
    /// <param name="function"></param>
    /// <param name="formulas"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private IVariable CallFunction(FunctionToken function, List<IFormula> formulas,int depth)
    {
        Debug($"EvalFT : {function}",depth);
        
        // 引数の数を確認する
        if (function.Parameters.Count != formulas.Count)
            throw new ArgumentException("parameter not match");
        
        // 引数を評価
        var variables = formulas.Select(f=>Evaluate(f,depth+1)).ToList();

        var dict = new Dictionary<string, IVariable>();
        for (int i = 0; i < function.Parameters.Count; i++)
        {
            var pName = function.Parameters[i].Name;
            dict[pName] = variables[i];　//TODO 参照渡し->名前の参照渡し？
        }

        // 関数を実行
        var result = Execute(function.Body,dict,depth);
        
        //結果を返す
        Debug($"-> {result}",depth);
        return result;
    }

    //TODO 名前と名前型は違う→Evaluateでおかしくなる
    private IVariable EvaluatePrimitiveFunction(PrimitiveFunction primitive, List<IFormula> parameters,int depth)
    {        
        Debug($"EvalP : {primitive}",depth);

        // とりあえず評価しておく
        var variables = parameters.Select(f=>Evaluate(f,depth+1)).ToList();

        IVariable result;
        switch (primitive.Function)
        {
            case PrimitiveFunction.Type.Assign:
            {
                if (variables.Count != 2)
                {
                    throw new ArgumentException("parameter not match");
                }

                if (variables[0] is not NameType nameType)
                {
                    throw new ArgumentException("assignの第一引数は名前型である必要があります。");
                }

                var name = nameType.Name;
                var value = variables[1];
                _runStack.Assign(name, value);
                
                //Console.WriteLine($"ASSIGNED {name} : {value}");
                
                result = value;
                break;
            }
            case PrimitiveFunction.Type.Put:
            {
                var str = string.Join(" ", variables);
                _outputFunction(str);
                result = new ConstantData<string>(str);
                break;
            }
            case PrimitiveFunction.Type.Input:
            {
                if (variables.Count != 0)
                    throw new ArgumentException("parameter not match");
                
                var str = _inputFunction();
                result = new ConstantData<string>(str ?? "");
                break;
            }
            default:
                throw new NotImplementedException();
        }
        
        Debug($"-> {result}",depth);
        return result;
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
            
            // TODO これ、Termが読めないことはあるのか？
            
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
            throw new Exception("中値演算子の数は項の数-1である必要があります");
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
           term.Expressions[index] is not VariableToken variable) // TODO 直接な関数定義は使えないの？
            return (false,index,null)!;

        var searchResult = _runStack.GetVariable(variable.Name);
        
        if(searchResult is not FunctionToken function ||
           function.Type != FunctionType.Mid)
            return (false,index,null)!;
        
        return (true,index+1,function);
    }

    private IFormula ReadFormula(ExpressionToken expr)
    {
        IFormula result;
        
        switch(expr)
        {
            case ConstantData<string> value:
                result = value;
                break;
            case ConstantData<int> value:
                result = value;
                break;
            case TermToken term:
            {
                int index = 0;
                var formulas = new List<IFormula>();
                while (-1 < index && index < term.Expressions.Count)
                {
                    IFormula formula;
                    (index,formula) = NextFormula(term,index);
                    formulas.Add(formula);
                }
                result = new Formula(formulas,new List<FunctionToken>());
                break;
            }
            case FunctionCallToken funcCall:
            {
                // 引数を取り出す   
                int index = 0;
                List<IFormula> parameters = new();
                while(-1 < index && index < funcCall.Parameters.Expressions.Count)
                {
                    IFormula formula;
                    (index,formula) = NextFormula(funcCall.Parameters,index);
                    parameters.Add(formula);
                }

                // 関数本体を取り出す
                IFormula function = ReadFormula(funcCall.Function);
                
                // 関数呼び出しとして終了
                result = new FunctionCall(function,parameters);
                break;
            }
            case FunctionToken func:
                result = func;
                break;
            case VariableToken variable:
            {
                var name = variable.Name;
                var searchResult = _runStack.GetVariable(name);

                if(searchResult is Void)
                {
                    // ;から始まるならば名前型
                    if (name[0] == ';')
                    {
                        result = new NameType(name.Substring(1));
                        break;
                    }
                    
                    // builtin
                    if (PrimitiveFunction.TryParse(name, out var primitiveFunction))
                    {
                        result = primitiveFunction;
                        break;
                    }
                    
                    // 数字の可能性
                    if(int.TryParse(name,out int value))
                    {
                        result = new ConstantData<int>(value);
                        break;
                    }

                    result = new UnknownVariable(name);
                    break;
                }

                // 変数が関数なら、関数をそのまま渡す
                if(searchResult is FunctionToken func)
                {
                    result = func;
                    break;
                }

                result = searchResult;
                break;
            }
            default:
                throw new NotImplementedException(expr.ToString());
        }

        return result;
    }

    private (int index,IFormula term) NextTerm(TermToken target,int index)
    {
        var exprs = target.Expressions;

        List<FunctionToken> prefixFuncs;
        (index,prefixFuncs) = ReadFixFunctions(target,index,true);

        // 前置演算子だけで終了した
        if(index == -1 && prefixFuncs.Count != 0)
        {
            //TODO エラーではなくて、関数を合成する？
            throw new Exception($"There are {prefixFuncs.Count} prefix operators, but formula is not found.");
        }
        
        // 本体を読む
        IFormula midTerm = ReadFormula(exprs[index]);
        
        // 後置演算子を読む
        List<FunctionToken> suffixFuncs;
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

    /// <summary>
    /// 前置演算子か後置演算子の列を
    /// </summary>
    /// <param name="term"></param>
    /// <param name="index"></param>
    /// <param name="isPrefixMode"></param>
    /// <returns></returns>
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