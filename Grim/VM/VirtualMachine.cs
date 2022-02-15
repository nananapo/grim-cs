using System.Diagnostics;
using Grim.AST;
using Grim.Errors;
using Grim.Token;

namespace Grim.VM;

public class VirtualMachine
{

    private readonly RunStack _runStack = new();

    public bool EnableLogging = false;

    private readonly Action<string> _outputFunction;

    private readonly Func<string?> _inputFunction;

    private readonly AbstractSyntaxTree _ast;

    public VirtualMachine(Action<string>? outputFunc = null,Func<string?>? inputFunc = null,bool enableLogging = false)
    {
        _outputFunction = outputFunc ?? Console.Write;
        _inputFunction = inputFunc ?? Console.ReadLine;
        
        EnableLogging = enableLogging;
        _ast = new(_runStack,enableLogging:enableLogging);
    }
    
    [Conditional("DEBUG")] 
    private void Debug(string text, int depth)
    {
        if (!EnableLogging) return;
        
        var spaces = "";
        for (int i = 0; i < depth; i++)
            spaces += "  ";
        
        Console.WriteLine($"[VM]  {depth} {spaces}{text}");
    }
    
    public IVariable Execute(List<IToken> exprs,Scope? lexicalScope = null,Dictionary<string,IVariable>? variables = null,int depth = 0)
    {
        // 省略注意
        if (lexicalScope == null)
        {
            lexicalScope = _runStack.Root;
        }
        
        // スタックする
        _runStack.Push(lexicalScope);
      
        Debug($"STACK PUSH[{_runStack.StackCount}]",depth);

        // 引数を環境に入れる
        // TODO 静的スコープ
        if (variables != null)
        {
            foreach (var (name,variable) in variables)
            {
                _runStack.Now.Set(name,variable);
            }
        }

        IVariable result = Void.Instance;
        
        int index = 0;
        while (-1 < index && index < exprs.Count)
        {
            IFormula formula;
#if DEBUG
            if(EnableLogging) Console.WriteLine();
#endif
            (index,formula) = _ast.NextFormula(exprs,index,0);
#if DEBUG
            if(EnableLogging) Console.WriteLine();
#endif
            result = Evaluate(formula,depth+1);
        }

        Debug($"STACK POP[{_runStack.StackCount}]",depth);
        _runStack.Pop();
        return result;
    }

    private IVariable Evaluate(IFormula target, int depth)
    {
        //Debug($"EA {target}",depth);

        if (target is Unknown unknown)
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
            BuiltInFunction func => func,
            
            Formula formula => EvaluateFormula(formula, depth),
            FunctionCall call => Evaluate(call, depth),
            Term modifierTerm => EvaluateTerm(modifierTerm,depth),
            Function function => function,
            NameType nameType => nameType,
            Void v => v,
            _ => throw new NotImplementedException(target.GetType().FullName)
        };
    }

    private IVariable EvaluateFormula(Formula formula,int depth)
    {
        Debug($"EvalF : {formula}",depth);
        
        IVariable result = Void.Instance;
        
        if (formula.Terms.Count == 0)
        {
            // TODO 関数として返す?
            // TODO 関数の塊として返す?
            if (formula.MidOperators.Count != 0)
                throw new Exception("中値演算子が不正な位置にあります");

            Debug($"-> {result}",depth);
            return result;
        }
        
        var terms = formula.Terms.ToList();
        var ops = formula.MidOperators.ToList();
        
        while (terms.Count > 1 && ops.Count > 0)
        {
            var max = ops.Select(v => Math.Abs(v.Priority)).Max();
            
            var lefts = ops.Where(v => v.Priority == max && v.IsLeftAssociative).ToList();
            var rights = ops.Where(v => v.Priority == max && !v.IsLeftAssociative).ToList();

            // すべてが右結合の場合
            if (lefts.Count == 0)
            {
                // 右結合は後ろから処理する
                // TODO ソート必要？
                while (rights.Count > 0)
                {
                    var fun = rights[^1];
                    var opIndex = ops.IndexOf(fun);
                    var t1 = terms[opIndex];
                    var t2 = terms[opIndex + 1];
                    
                    terms[opIndex] = CallFunction(fun, new List<IFormula> {t1, t2}, depth + 1);
                    
                    // 削除
                    terms.RemoveAt(opIndex+1);
                    ops.RemoveAt(opIndex);
                    rights.RemoveAt(rights.Count-1);
                }
            }
            // すべてが左結合の場合
            else if (rights.Count == 0)
            {
                // 左結合は前から処理する
                // TODO ソート必要？
                while (lefts.Count > 0)
                {
                    var fun = lefts[0];
                    var opIndex = ops.IndexOf(fun);
                    var t1 = terms[opIndex];
                    var t2 = terms[opIndex + 1];
                    
                    terms[opIndex] = CallFunction(fun, new List<IFormula> {t1, t2}, depth + 1);
                    
                    // 削除
                    terms.RemoveAt(opIndex+1);
                    ops.RemoveAt(opIndex);
                    lefts.RemoveAt(0);
                }
            }
            // 混ざっている
            // 左結合→右結合
            else
            {
                // 左結合を処理
                while (lefts.Count > 0)
                {
                    var fun = lefts[0];
                    var opIndex = ops.IndexOf(fun);
                    var t1 = terms[opIndex];
                    var t2 = terms[opIndex + 1];
                    
                    terms[opIndex] = CallFunction(fun, new List<IFormula> {t1, t2}, depth + 1);
                    
                    // 削除
                    terms.RemoveAt(opIndex+1);
                    ops.RemoveAt(opIndex);
                    lefts.RemoveAt(0);
                }
                // 右結合を処理
                while (rights.Count > 0)
                {
                    var fun = rights[^1];
                    var opIndex = ops.IndexOf(fun);
                    var t1 = terms[opIndex];
                    var t2 = terms[opIndex + 1];
                    
                    terms[opIndex] = CallFunction(fun, new List<IFormula> {t1, t2}, depth + 1);
                    
                    // 削除
                    terms.RemoveAt(opIndex+1);
                    ops.RemoveAt(opIndex);
                    rights.RemoveAt(rights.Count-1);
                }
            }
        }
        
        // 残った項を前から処理する
        while(terms.Count > 0)
        {
            result = Evaluate(terms[0], depth + 1);
            terms.RemoveAt(0);
        }
        
        Debug($"-> {result}",depth);
        return result;
    }

    private IVariable Evaluate(FunctionCall funcCall,int depth)
    {
        Debug($"EvalC : {funcCall}",depth);
        
        IVariable result;
        
        // 関数を取得する
        var body = Evaluate(funcCall.Lambda, depth+1);
        
        // 正常に取得できた場合、実行する
        if (body is Function function)
        {
            result = CallFunction(function,funcCall.Parameters,depth+1);
        }
        // builtin
        else if (body is BuiltInFunction primitive)
        {
            result = CallPrimitiveFunction(primitive, funcCall.Parameters, depth + 1);
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
    private IVariable EvaluateTerm(Term term,int depth)
    { 
        Debug($"EvalMT : {term}",depth);

        // 中心を評価
        var result = Evaluate(term.MidFormula,depth+1);
        
        // prefixとsuffixを処理
        int pIndex = 0;
        int sIndex = 0;
        while (pIndex < term.PrefixFuncs.Count && sIndex < term.SuffixFuncs.Count)
        {
            var pf = term.PrefixFuncs[pIndex];
            var sf = term.SuffixFuncs[sIndex];
            
            // prefixのほうが優先度が高い
            if (pf.Priority > sf.Priority)
            {
                result = CallFunction(pf, new List<IFormula> {result}, depth + 1);
                pIndex++;
                continue;
            }
            
            // suffixのほうが優先度が高い
            if(pf.Priority < sf.Priority)
            {
                result = CallFunction(sf, new List<IFormula> {result}, depth + 1);
                sIndex++;
                continue;
            }
            
            // ↓優先度が同じ
            // 両方右結合なら後置
            if (pf.IsLeftAssociative == sf.IsLeftAssociative && !pf.IsLeftAssociative)
            {
                result = CallFunction(sf, new List<IFormula> {result}, depth + 1);
                sIndex++;
            }
            // 両方左結合か、結合が違うなら前置
            else
            {
                result = CallFunction(pf, new List<IFormula> {result}, depth + 1);
                pIndex++;
            }
        }

        // prefixが余ってるなら処理
        while (pIndex < term.PrefixFuncs.Count)
        {
            result = CallFunction(term.PrefixFuncs[pIndex], new List<IFormula> {result}, depth + 1);
            pIndex++;
        }

        // suffixが余ってるなら処理
        while (sIndex < term.SuffixFuncs.Count)
        {
            result = CallFunction(term.SuffixFuncs[sIndex], new List<IFormula> {result}, depth + 1);
            sIndex++;
        }
        
        Debug($"-> {result}",depth);
        return result;
    }

    /// <summary>
    /// FunctionTokenを評価する
    /// TODO 関数なら合成
    /// </summary>
    /// <param name="function"></param>
    /// <param name="formulas"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private IVariable CallFunction(Function function, IList<IFormula> formulas,int depth)
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
        var result = Execute(new List<IToken>{function.Body},function.DefinedScope,dict,depth);
        
        //結果を返す
        Debug($"-> {result}",depth);
        return result;
    }

    /// <summary>
    /// ビルトイン関数を呼ぶ
    /// </summary>
    /// <param name="builtIn"></param>
    /// <param name="parameters"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    private IVariable CallPrimitiveFunction(BuiltInFunction builtIn, List<IFormula> parameters,int depth)
    {        
        Debug($"EvalP : {builtIn}",depth);

        // とりあえず評価しておく
        var variables = parameters.Select(f=>Evaluate(f,depth+1)).ToList();

        IVariable result;
        switch (builtIn.Function)
        {
            case BuiltInFunctionType.Assign:
            {
                if (variables.Count != 2)
                {
                    throw new ArgumentException("parameter not match");
                }

                if (variables[0] is not NameType nameType)
                {
                    throw new ArgumentException("assignの第一引数は名前型である必要があります。");
                }

                var value = variables[1];
                nameType.Scope.Set(nameType.Name, value);

                Debug("ASSIGNED " + nameType.Name + " : " + value, depth);

                result = value;
                break;
            }
            case BuiltInFunctionType.Put:
            {
                var str = string.Join(" ", variables);
                _outputFunction(str);
                result = new ConstantData<string>(str);
                break;
            }
            case BuiltInFunctionType.Input:
            {
                if (variables.Count != 0)
                    throw new ArgumentException("parameter not match");

                var str = _inputFunction();
                result = new ConstantData<string>(str ?? "");
                break;
            }
            case BuiltInFunctionType.Add:
            {
                if (variables.Count != 2)
                    throw new ArgumentException("parameter not match");

                var va1 = variables[0];
                var va2 = variables[1];

                if (va1 is ConstantData<string> ds1)
                {
                    result = va2 switch
                    {
                        ConstantData<string> ds2 => new ConstantData<string>(ds1.Value + ds2.Value),
                        ConstantData<int> di2 => new ConstantData<string>(ds1.Value + di2.Value),
                        _ => throw new ParameterTypeException("__add", va1, va2)
                    };
                }
                else if (va1 is ConstantData<int> di1)
                {
                    result = va2 switch
                    {
                        ConstantData<string> ds2 => new ConstantData<string>(di1.Value + ds2.Value),
                        ConstantData<int> di2 => new ConstantData<int>(di1.Value + di2.Value),
                        _ => throw new ParameterTypeException("__add", va1, va2)
                    };
                }
                else
                {
                    throw new ParameterTypeException("__add", va1, va2);
                }

                break;
            }
            case BuiltInFunctionType.Negate:
            {
                if (variables.Count != 1)
                    throw new ArgumentException("parameter not match");

                var va = variables[0];

                if (va is ConstantData<int> data)
                {
                    result = new ConstantData<int>(data.Value * -1);
                }
                else
                {
                    throw new ParameterTypeException("__negate", va);
                }

                break;
            }
            case BuiltInFunctionType.Equal:
            {
                if (variables.Count != 2)
                    throw new ArgumentException("parameter not match");

                var va1 = variables[0];
                var va2 = variables[1];

                result = new ConstantData<int>(va1.Equals(va2) ? 1 : 0);
                break;
            }
            case BuiltInFunctionType.If:
            {
                if (variables.Count != 2 &&
                    variables.Count != 3)
                    throw new ArgumentException("parameter not match");
                
                var va1 = variables[0];
                var va2 = variables[1];

                if (va1 is not ConstantData<int> f1)
                {
                    throw new ParameterTypeException("__if", va1);
                }
                
                if (va2 is not Function f2)
                {
                    throw new ParameterTypeException("__if", va2);
                }
                
                if (f1.Value == 1)
                {
                    CallFunction(f2, Array.Empty<IFormula>(), depth + 1);
                }
                else if(variables.Count == 3)
                {
                    var va3 = variables[2];
                    if (va3 is Function f3)
                    {
                        CallFunction(f3, Array.Empty<IFormula>(), depth + 1);
                    }
                    else
                    {
                        throw new ParameterTypeException("__if", va3);
                    }
                }

                result = Void.Instance;
                break;
            }
            case BuiltInFunctionType.While:
            {
                if (variables.Count != 2)
                    throw new ArgumentException("parameter not match");
                
                var va1 = variables[0];
                var va2 = variables[1];

                if (va1 is not Function f1)
                {
                    throw new ParameterTypeException("__if", va1);
                }
                
                if (va2 is not Function f2)
                {
                    throw new ParameterTypeException("__if", va2);
                }

                while(true)
                {
                    // 評価
                    IVariable current = CallFunction(f1, Array.Empty<IFormula>(), depth + 1);
                    if (current is not ConstantData<int> {Value: 1})
                    {
                        break;
                    }
                    
                    // 呼ぶ
                    CallFunction(f2, Array.Empty<IFormula>(), depth + 1);
                }

                result = Void.Instance;
                break;
            }
            default:
                throw new NotImplementedException();
        }
        
        Debug($"-> {result}",depth);
        return result;
    }
}