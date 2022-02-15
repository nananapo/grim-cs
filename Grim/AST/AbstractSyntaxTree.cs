using System.Diagnostics;
using Grim.Errors;
using Grim.Token;
using Grim.VM;
using Void = Grim.VM.Void;

namespace Grim.AST;
public class AbstractSyntaxTree
{

    private readonly RunStack _runStack;

    public AbstractSyntaxTree(RunStack runStack)
    {
        _runStack = runStack;
    }
    
    [Conditional("DEBUG")] 
    private void Debug(string text, int depth)
    {
        var spaces = " ";
        for (int i = 0; i < depth; i++)
            spaces += "  ";
        
        Console.WriteLine($"[AST] {depth} {spaces}{text}");
    }
    
    public (int index,IFormula formula) NextFormula(in List<IToken> exprs,int index,int depth)
    {
        Debug($"NextFormula[{index}] : " + string.Join(",", exprs.Skip(index)),depth);

        List<IFormula> terms = new ();
        List<Function> midOperators = new ();

        while(-1 < index && index < exprs.Count)
        {
            // Termを1つ読む
            IFormula term;
            (index, term) = NextTerm(exprs, index,depth+1);
            terms.Add(term);
            
            // 中値演算子を1つ読む
            bool isMidOperator;
            Function midOp;
            (isMidOperator,index,midOp) = NextMidOperator(exprs,index);
            
            //Debug($"EndMOP -> {isMidOperator}",depth);
            
            if(!isMidOperator)
                break;
            
            midOperators.Add(midOp);
        } 
        
        // 中値演算子の数が合わないならエラー
        if (terms.Count - 1 != midOperators.Count)
        {
            throw new ParseException($"中値演算子の数は項の数-1である必要があります\n 項の数 : {terms.Count}\n 中値演算子の数 : {midOperators.Count}");
        }

        IFormula result;

        // Termが一つならそれを返す
        if (midOperators.Count == 0 && terms.Count == 1)
        {
            result = terms[0];
        }
        else
        {
            result = new Formula(terms,midOperators);
        }

        Debug($"-> {result}",depth);

        return (index, result);
    }

    private (bool isMidOperator,int index,Function midOperator) NextMidOperator(in List<IToken> exprs,int index)
    {
        // 範囲チェック
        if(index < 0 || index >= exprs.Count)
            return (false,index,null)!;

        // 関数定義
        if (exprs[index] is FunctionToken functionToken)
        {
            // 中値演算子ではないなら終了
            if(functionToken.Type != FunctionType.Mid)
                return (false,index,null)!;

            // Functionを返す
            return (true, index + 1, new Function(_runStack.Now, functionToken));
        }
        
        // 変数でないなら終了
        if (exprs[index] is not VariableToken variable)
        {
            return (false,index,null)!;
        }

        // 関数ではないか
        // 中値演算子ではないなら終了
        if(!_runStack.TryGetVariable(variable.Name,out var searchResult) ||
           searchResult is not Function function ||
           function.Type != FunctionType.Mid)
            return (false,index,null)!;
        
        // Functionを返す
        return (true,index+1,function);
    }

    /// <summary>
    /// 項を読む
    /// </summary>
    /// <param name="exprs"></param>
    /// <param name="index"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
    private (int index,IFormula term) NextTerm(in List<IToken> exprs,int index,int depth)
    {
        List<Function> prefixFuncs;
        (index,prefixFuncs) = ReadFixFunctions(exprs,index,true);

        // 前置演算子だけで終了
        if(index < 0 || index >= exprs.Count)
        {
            // 前置演算子が空ならVoidを返す
            if (prefixFuncs.Count == 0)
            {
                return (index, Void.Instance);
            }
            
            // TODO 一気に適用できる演算子列を返したい
            // とりあえず最後の前置演算子を返す
            return (index, prefixFuncs[^1]);
        }
        
        // デリミタなら終了
        if (exprs[index] is DelimiterToken)
        {
            // 前置演算子が空ならデリミタをスキップして読み込みなおす
            if (prefixFuncs.Count == 0)
            {
                Debug("Skip Delimiter;",depth);
                return NextTerm(exprs,index+1,depth);
            }
            
            // TODO 一気に適用できる演算子列を返したい
            // とりあえず最後の前置演算子を返す
            return (index, prefixFuncs[^1]);
        }

        // 本体を読む
        IFormula midTerm = ReadMidTerm(exprs[index],depth+1);
        
        // 後置演算子を読む
        List<Function> suffixFuncs;
        (index,suffixFuncs) = ReadFixFunctions(exprs,index+1,false);

        // 前置演算子も後置演算子もないならそのまま返す
        if (prefixFuncs.Count == 0 &&
            suffixFuncs.Count == 0)
        {
            return (index, midTerm);
        }
        
        // 演算子で修飾して返す
        return (index,new Term(prefixFuncs,midTerm,suffixFuncs));
    }

    /// <summary>
    /// 項を読む
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
    private IFormula ReadMidTerm(IToken expr,int depth)
    {
        Debug($"ReadMidTerm : {expr}",depth);
        
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
                /* TODO (A B)みたいなのは禁止
                var terms = new List<IFormula>();
                
                while (-1 < index && index < term.Expressions.Count)
                {
                    IFormula formula;
                    (index,formula) = NextFormula(term.Expressions,index,depth+1);
                    terms.Add(formula);
                }
                
                result = new Formula(terms,new List<Function>());
                */
                (index,result) = NextFormula(term.Expressions,0,depth+1);
                if (index != -1)
                {
                    throw new ParseException("括弧の中で、2つ以上の式を並べることはできません。");
                }
                break;
            }
            case FunctionCallToken funcCall:
            {
                // 引数を取り出す   
                int index = 0;
                List<IFormula> parameters = new();
                while(-1 < index && index < funcCall.Parameters.Count)
                {
                    IFormula formula;
                    (index,formula) = NextFormula(funcCall.Parameters,index,depth+1);
                    parameters.Add(formula);
                }

                // 関数本体を取り出す
                IFormula function = ReadMidTerm(funcCall.Function,depth+1);
                
                // 関数呼び出しとして終了
                result = new FunctionCall(function,parameters);
                break;
            }
            case FunctionToken func:
                result = new Function(_runStack.Now,func);
                break;
            case VariableToken variable:
            {
                var name = variable.Name;

                // 変数を見つけたら返す
                if(_runStack.TryGetVariable(name,out var searchResult))
                {
                    result = searchResult;
                    break;
                }

                // ;から始まるならば名前型
                // TODO Tokenizerでどうにかしたい
                if (name[0] == Tokenizer.NameTypePrefix)
                {
                    if (name.Length == 1)
                    {
                        throw new ParseException(Tokenizer.NameTypePrefix + "の後には1文字以上の文字が必要です");
                    }
                        
                    result = new NameType(name.Substring(1),_runStack.Now);
                    break;
                }
                    
                // builtin
                if (BuiltInFunctionHelper.TryParse(name, out var builtInType))
                {
                    result = new Function(builtInType);
                    break;
                }
                    
                // 数字の可能性
                if(int.TryParse(name,out int value))
                {
                    result = new ConstantData<int>(value);
                    break;
                }

                result = new Unknown(name);
                break;
            }
            default:
                throw new NotImplementedException(expr.ToString());
        }

        Debug($"-> {result}",depth);

        return result;
    }

    /// <summary>
    /// 前置演算子か後置演算子の列を読む
    /// </summary>
    /// <param name="exprs"></param>
    /// <param name="index"></param>
    /// <param name="isPrefixMode"></param>
    /// <returns></returns>
    private (int index, List<Function> functions) ReadFixFunctions(in List<IToken> exprs,int index,bool isPrefixMode)
    {
        var functions = new List<Function>();
        
        for(;index < exprs.Count;index++)
        {
            var expr = exprs[index];

            // ここで定義されている
            if (expr is FunctionToken functionToken)
            {
                // 前置演算子でも後置演算子でもない場合は終了
                if(!(functionToken.Type == FunctionType.Prefix && isPrefixMode) 
                   && !(functionToken.Type == FunctionType.Suffix && !isPrefixMode))
                {
                    return (index,functions);
                }
                
                functions.Add(new Function(_runStack.Now,functionToken));
                continue;
            }
            
            // 変数でないなら終了
            if(expr is not VariableToken variable)
            {
                return (index,functions);
            }
            
            // 見つからなかったら終了
            // 関数ではないか
            // 前置演算子でも後置演算子でもない場合は終了
            if(!_runStack.TryGetVariable(variable.Name, out var searchResult) ||
                searchResult is not Function func ||
               !(func.Type == FunctionType.Prefix && isPrefixMode) 
               && !(func.Type == FunctionType.Suffix && !isPrefixMode))
            {
                return (index,functions);
            }

            functions.Add(func);
        }

        return (-1,functions);
    }
}