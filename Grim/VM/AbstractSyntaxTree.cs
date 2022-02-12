using Grim.Token;

namespace Grim.VM;

public class AbstractSyntaxTree
{

    private readonly RunStack _runStack;

    public AbstractSyntaxTree(RunStack runStack)
    {
        _runStack = runStack;
    }

    private void Debug(string text, int depth)
    {
        var spaces = " ";
        for (int i = 0; i < depth; i++)
            spaces += "  ";
        
        Console.WriteLine($"[AST] {depth} {spaces}{text}");
    }
    
    public (int index,IFormula formula) NextFormula(List<ExpressionToken> exprs,int index,int depth)
    {
        Debug($"NextFormula : " + string.Join(",", exprs.Skip(index)),depth);

        List<IFormula> terms = new ();
        List<FunctionToken> midOperators = new ();

        while(-1 < index && index < exprs.Count)
        {
            // Termを1つ読む
            IFormula term;
            (index, term) = NextTerm(exprs, index,depth+1);
            terms.Add(term);
            
            // 中値演算子を1つ読む
            bool isMidOperator;
            FunctionToken midOp;
            (isMidOperator,index,midOp) = NextMidOperator(exprs,index);
            
            if(!isMidOperator)
                break;
            
            midOperators.Add(midOp);
        } 
        
        // 中値演算子の数が合わないならエラー
        if (terms.Count - 1 != midOperators.Count)
        {
            throw new Exception($"中値演算子の数は項の数-1である必要があります\n 項の数 : {terms.Count}\n 中値演算子の数 : {midOperators.Count}");
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

    private (bool isMidOperator,int index,FunctionToken midOperator) NextMidOperator(List<ExpressionToken> exprs,int index)
    {
        if(index < 0 || 
           index >= exprs.Count ||
           exprs[index] is not VariableToken variable) // TODO 直接な関数定義は使えないの？
            return (false,index,null)!;

        var searchResult = _runStack.GetVariable(variable.Name);
        
        if(searchResult is not FunctionToken function ||
           function.Type != FunctionType.Mid)
            return (false,index,null)!;
        
        return (true,index+1,function);
    }

    private (int index,IFormula term) NextTerm(List<ExpressionToken> exprs,int index,int depth)
    {
        List<FunctionToken> prefixFuncs;
        (index,prefixFuncs) = ReadFixFunctions(exprs,index,true);

        // 前置演算子だけで終了した
        if(index == -1 && prefixFuncs.Count != 0)
        {
            //TODO エラーではなくて、関数を合成する？
            throw new Exception($"There are {prefixFuncs.Count} prefix operators, but formula is not found.");
        }
        
        // 本体を読む
        IFormula midTerm = ReadFormula(exprs[index],depth+1);
        
        // 後置演算子を読む
        List<FunctionToken> suffixFuncs;
        (index,suffixFuncs) = ReadFixFunctions(exprs,index+1,false);

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
    /// 式を読む
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
    private IFormula ReadFormula(ExpressionToken expr,int depth)
    {

        Debug($"ReadFormula : {expr}",depth);
        
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
                    (index,formula) = NextFormula(term.Expressions,index,depth+1);
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
                while(-1 < index && index < funcCall.Parameters.Count)
                {
                    IFormula formula;
                    (index,formula) = NextFormula(funcCall.Parameters,index,depth+1);
                    parameters.Add(formula);
                }

                // 関数本体を取り出す
                IFormula function = ReadFormula(funcCall.Function,depth+1);
                
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
    private (int index, List<FunctionToken> functions) ReadFixFunctions(List<ExpressionToken> exprs,int index,bool isPrefixMode)
    {
        var functions = new List<FunctionToken>();
        
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