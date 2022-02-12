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

    public VirtualMachine(Action<string>? outputFunc = null,Func<string?>? inputFunc = null)
    {
        _outputFunction = outputFunc ?? Console.Write;
        _inputFunction = inputFunc ?? Console.ReadLine;
        _ast = new(_runStack);
    }

    private void Debug(string text, int depth)
    {
        if (!EnableLogging) return;
        
        var spaces = " ";
        for (int i = 0; i < depth; i++)
            spaces += "  ";
        
        Console.WriteLine(depth + spaces + text);
    }

    public IVariable Execute(List<ExpressionToken> exprs,Dictionary<string,IVariable>? variables = null,int depth = 0)
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

        IVariable result = Void.Create();
        
        int index = 0;
        while (index < exprs.Count)
        {
            IFormula formula;
            (index,formula) = _ast.NextFormula(exprs,index);
            result = Evaluate(formula,depth+1);
        }
        
        _runStack.Pop();
        return result;
    }

    private IVariable Evaluate(IFormula target, int depth)
    {
        Debug($"EA {target}",depth);

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
    private IVariable EvaluateTerm(ModifierTerm term,int depth)
    { 
        Debug($"EvalMT : {term}",depth);

        // 中心を評価
        var result = Evaluate(term.Term,depth+1);
        
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
        var result = Execute(new List<ExpressionToken>{function.Body},dict,depth);
        
        //結果を返す
        Debug($"-> {result}",depth);
        return result;
    }

    /// <summary>
    /// ビルトイン関数を呼ぶ
    /// </summary>
    /// <param name="primitive"></param>
    /// <param name="parameters"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    private IVariable CallPrimitiveFunction(PrimitiveFunction primitive, List<IFormula> parameters,int depth)
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
            case PrimitiveFunction.Type.Add:
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
                        _ => throw new AddException(va1, va2)
                    };
                }
                else if (va1 is ConstantData<int> di1)
                {
                    result = va2 switch
                    {
                        ConstantData<string> ds2 => new ConstantData<string>(di1.Value + ds2.Value),
                        ConstantData<int> di2 => new ConstantData<int>(di1.Value + di2.Value),
                        _ => throw new AddException(va1, va2)
                    };
                }
                else
                {
                    throw new AddException(va1, va2);
                }
                
                break;
            }
            default:
                throw new NotImplementedException();
        }
        
        Debug($"-> {result}",depth);
        return result;
    }
}