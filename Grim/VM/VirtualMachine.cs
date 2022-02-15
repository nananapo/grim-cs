using System.Diagnostics;
using Grim.AST;
using Grim.Errors;
using Grim.Token;

namespace Grim.VM;

public class VirtualMachine
{

    private readonly RunStack _runStack = new();

    private readonly Action<string> _outputFunction;

    private readonly Func<string?> _inputFunction;

    private readonly AbstractSyntaxTree _ast;

    public VirtualMachine(Action<string>? outputFunc = null,Func<string?>? inputFunc = null)
    {
        _outputFunction = outputFunc ?? Console.Write;
        _inputFunction = inputFunc ?? Console.ReadLine;
        _ast = new(_runStack);
    }
    
    [Conditional("DEBUG")] 
    private void Debug(string text, int depth)
    {
        var spaces = "";
        for (int i = 0; i < depth; i++)
            spaces += "  ";
        
        Console.WriteLine($"[VM]  {depth} {spaces}{text}");
    }
    
    public IVariable Execute(List<IToken> exprs,Dictionary<string,IVariable>? variables = null,int depth = 0,int lexicalScopeId = 0,bool enableStack = true)
    {

        if (enableStack)
        {
            // スタックする
            _runStack.Push(lexicalScopeId);
        }
      
        Debug($"STACK PUSH[{_runStack.StackCount}]",depth);

        // 引数を環境に入れる
        if (variables != null)
        {
            foreach (var (name,variable) in variables)
            {
                _runStack.SetVariable(_runStack.Now,name,variable);
            }
        }

        IVariable result = Void.Instance;
        
        int index = 0;
        while (-1 < index && index < exprs.Count)
        {
            IFormula formula;
#if DEBUG
            Console.WriteLine();
#endif
            (index,formula) = _ast.NextFormula(exprs,index,0);
#if DEBUG
            Console.WriteLine();
#endif
            result = Evaluate(formula,depth+1);
        }

        Debug($"STACK POP[{_runStack.StackCount}]",depth);

        if (enableStack)
        {
            _runStack.Pop();
        }
        return result;
    }

    private IVariable Evaluate(IFormula target, int depth)
    {
        //Debug($"EA {target}",depth);
        
        // 既に評価済み
        if (target is ConstantData<string> ||
            target is ConstantData<int> ||
            target is Function ||
            target is NameType ||
            target is Void)
        {
            return (IVariable)target;
        }

        // 関数呼び出し
        if (target is FunctionCall functionCall)
        {
            // 関数を取得する
            var body = Evaluate(functionCall.Lambda, depth+1);
        
            // 正常に取得できなかった
            if (body is not Function function)
            {
                throw new  Exception(body.GetType().Name + "は関数ではないため呼ぶことができません。");
            }
            
            return CallFunction(function,functionCall.Parameters,depth+1);
        }

        // 不明な変数
        if (target is Unknown unknown)
        {
            // 不明ならエラー
            if (!_runStack.TryGetVariable(unknown.Name,out var result))
            {
                throw new Exception($"\"{unknown.Name}\"を解決できませんでした");
            }
            
            return result;
        }
        
        return target switch
        {
            Formula formula => EvaluateFormula(formula, depth),
            Term modifierTerm => EvaluateTerm(modifierTerm,depth),
            _ => throw new NotImplementedException(target.GetType().FullName)
        };
    }

    private IVariable EvaluateFormula(Formula formula,int depth)
    {
        Debug($"EvalF : {formula}",depth);
        
        IVariable result = Void.Instance;

        // 項が0個
        if (formula.Terms.Count == 0)
        {
            if (formula.MidOperators.Count == 0)
            {
                result = Void.Instance;
            }
            // 最後の関数を返す
            else if (formula.MidOperators.Count == 1)
            {
                result = formula.MidOperators[^1];
            }
            else
            {
                throw new Exception("中値演算子が不正な位置にあります");
            }
            
            Debug($"-> {result}",depth);
            return result;
        } 
        else if (formula.Terms.Count == 1)
        {
            result = Evaluate(formula.Terms[0], depth + 1);
            Debug($"-> {result}",depth);
            return result;
        }

        // priorityで辞書にする
        var priorityAssociative = new Dictionary<int, int>();// -1 左 , 0 混在 , 1  右
        var priorityDict = new Dictionary<int, List<Function>>();
        
        foreach(var op in formula.MidOperators)
        {
            if (!priorityDict.ContainsKey(op.Priority))
            {
                priorityDict[op.Priority] = new(){op};
                priorityAssociative[op.Priority] = op.IsLeftAssociative ? -1 : 1;
            }
            else
            {
                priorityDict[op.Priority].Add(op);
                
                var now = priorityAssociative[op.Priority];
                var to = op.IsLeftAssociative ? -1 : 1;
                if (now != 0 && now != to)
                {
                    priorityAssociative[op.Priority] = 0;
                }
            }
        }
        
        //priorityを昇順にする
        var priorities = priorityDict.Keys.ToList();
        priorities.Sort();
        priorities.Reverse();

        foreach (var priority in priorities)
        {
            var ops = priorityDict[priority];
            var associative = priorityAssociative[priority];

            for (int i = 0; i < ops.Count; i++)
            {
                // 右結合
                // 混在は左結合
                var fun = ops[associative == -1 ? ^(i + 1) : i];
                var opIndex = formula.MidOperators.IndexOf(fun);

                var t1 = formula.Terms[opIndex];
                var t2 = formula.Terms[opIndex + 1];

                formula.Terms[opIndex] = CallFunction(fun, new List<IFormula> {t1, t2}, depth + 1);

                // 削除
                formula.Terms.RemoveAt(opIndex + 1);
                formula.MidOperators.RemoveAt(opIndex);
            }
        }

        // 残った項を前から処理する
        foreach (var t in formula.Terms)
        {
            result = Evaluate(t, depth + 1);
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
    /// <param name="parameters"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private IVariable CallFunction(Function function, IList<IFormula> parameters,int depth)
    {
        Debug($"EvalFT : {function}",depth);
        
        // 引数の数が多すぎる
        var nc = function.AppliedParameterVariables.Count + parameters.Count;
        if (nc > function.Parameters.Count)
        {
            throw new ArgumentException($"関数に対して多すぎる引数を割り当てようとしています\n expected : {function.Parameters.Count}\n actual : {nc}");
        }

        IVariable result;
        
        // 引数の数が足りないなら一部を適用して関数を返す
        if (nc < function.Parameters.Count)
        {
            // 新しく割り当てる引数の数が0個ならそのまま返す
            if (parameters.Count == 0)
            {
                result = function;
            }
            // 割り当てる
            else
            {
                // リストに追加
                var applied = new List<IVariable>(function.AppliedParameterVariables);
                foreach (var formula in parameters)
                {
                    var variable = Evaluate(formula, depth + 1);
                    applied.Add(variable);
                }

                // 新しくFunctionを生成
                if (function.Type == FunctionType.BuiltIn)
                {
                    result = new Function(function.BuiltInFunctionType);
                }
                else
                {
                    result = new Function(function.DefinedScopeId,function.FunctionToken);
                }

                ((Function) result).AppliedParameterVariables = applied;
            }

            Debug($"-> {result}",depth);
            return result;
        }
        
        // 引数の数がピッタリ

        // 引数がない
        if (nc == 0)
        {
            if (function.Type == FunctionType.BuiltIn)
            {
                result = CallPrimitiveFunction(function.BuiltInFunctionType, Array.Empty<IVariable>(), depth + 1);
            }
            else
            {
                result = Execute(function.Body, depth: depth + 1, lexicalScopeId: function.DefinedScopeId);
            }
        }
        // 新しく適用されたもののみ
        else
        {
            List<IVariable> variables = 
                nc == parameters.Count ? new () : new List<IVariable>(function.AppliedParameterVariables);
            
            // 新しく割り当て
            foreach (var t in parameters)
            {
                variables.Add(Evaluate(t,depth+1));
            }
            
            // 実行
            if (function.Type == FunctionType.BuiltIn)
            {
                result = CallPrimitiveFunction(function.BuiltInFunctionType, variables, depth + 1);
            }
            else
            {
                // 辞書にする
                var dict = new Dictionary<string, IVariable>();
                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    dict[function.Parameters[i]] = variables[i];
                }

                result = Execute(function.Body, dict, depth: depth + 1, lexicalScopeId: function.DefinedScopeId);
            }
            
        }

        //結果を返す
        Debug($"-> {result}",depth);
        return result;
    }

    /// <summary>
    /// ビルトイン関数を呼ぶ
    /// </summary>
    /// <param name="builtInFunctionType"></param>
    /// <param name="variables"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    private IVariable CallPrimitiveFunction(BuiltInFunctionType builtInFunctionType, IList<IVariable> variables,int depth)
    {        
        Debug($"EvalP : {builtInFunctionType}",depth);

        IVariable result;
        switch (builtInFunctionType)
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
                _runStack.SetVariable(nameType.DefinedScopeId,nameType.Name, value);

                Debug("ASSIGNED " + nameType.Name + " : " + value, depth);

                result = value;
                break;
            }
            case BuiltInFunctionType.Put:
            {
                string str = variables[0].ToString();
                _outputFunction(str);
                result = new ConstantData<string>(str);
                break;
            }
            case BuiltInFunctionType.PERROR:
            {
                string str = variables[0].ToString();
                Console.Error.Write(str);
                result = Void.Instance;
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
            case BuiltInFunctionType.ReadFile:
            {
                if (variables.Count != 1)
                    throw new ArgumentException("parameter not match");
                
                var va = variables[0];
                if (va is not ConstantData<string> data)
                {
                    throw new ParameterTypeException("__read", va);
                }

                var fileName = data.Value;
                
                // 読めなかったらVoid
                if (!File.Exists(fileName))
                {
                    result = Void.Instance;
                    break;
                }
                
                var str = File.ReadAllText(fileName);
                result = new ConstantData<string>(str);
                break;
            }
            case BuiltInFunctionType.Eval:
            {
                if (variables.Count != 1)
                    throw new ArgumentException("parameter not match");
                
                var va = variables[0];
                if (va is not ConstantData<string> data)
                {
                    throw new ParameterTypeException("__eval", va);
                }

                var program = data.Value;
                
                // stackなし呼び出し
                var tokenizer = new Tokenizer(program);
                var term = tokenizer.Tokenize();
                result = Execute(term,depth:depth+1,enableStack:false);
                
                break;
            }
            default:
                throw new NotImplementedException();
        }
        
        Debug($"-> {result}",depth);
        return result;
    }
}