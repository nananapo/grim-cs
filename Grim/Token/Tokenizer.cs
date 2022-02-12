using Grim.VM;

namespace Grim.Token;

public class Tokenizer
{
    
    public const string Symbol = "()\" \t\n";

    private readonly string _program;

    public Tokenizer(string program)
    {
        _program = program;
    }

    public List<ExpressionToken> Tokenize()
    {
        var (_,term) = ReadBody(0,"end",false);
        return term.Expressions;
    }

    private (int,FunctionToken) ReadFunctionDefinition(int index,FunctionType type)
    {

        int priority = -1;
        if(type != FunctionType.General)
        {
            string token;
            (index,token) = ReadToken(index);
            if(!int.TryParse(token,out priority))
                throw new Exception("Failed to parse operator priority.");
        }

        List<VariableToken> parameters = new ();
        if(index < _program.Length)
        {
            if(_program[index] == '(')
            {
                (index,parameters) = ReadFunctionParameterDefinition(index+1);
            }
        }
                
        TermToken exprs;
        (index,exprs) = ReadBody(index,"end",true);

        return (index,new FunctionToken(type,parameters,exprs,priority));
    }

    // スペース区切り
    private (int index,List<VariableToken> func) ReadFunctionParameterDefinition(int index)
    {
        var names = new List<string>();
        var tokens = new List<VariableToken>();

        while(index < _program.Length)
        {
            string token;
            (index,token) = ReadToken(index);

            switch(token)
            {
                case ")":
                    return (index,tokens);
                case "\"":
                case "fun":
                case "opp":
                case "opm":
                case "ops":
                    // TODO 他にも使えないtokenがあるはず
                    throw new Exception("Parameter illegal symbol");
            }
            
            if(index == -1)
                throw new Exception("Parameter EOF");

            if (names.Contains(token))
                throw new Exception("同じ名前の引数を複数個定義することはできません");

            names.Add(token);
            tokens.Add(new VariableToken(token));
        }

        return (index,tokens);
    }

    private (int index, TermToken terms) ReadBody(int index,string endSymbol,bool requireClose)
    {
        List<ExpressionToken> exprs = new ();

        while(index < _program.Length && index != -1)
        {
            string str;
            (index,str) = ReadToken(index);

            if(str == endSymbol)
            {
                return (index,new TermToken(exprs));
            }

            //Console.WriteLine(index + " : " + str);

            ExpressionToken expr;
            switch(str)
            {
                case "":
                case " ":
                case "\t":
                case "\n":
                    continue;
                case ")":
                    throw new Exception("Unexpected close )");
                case "(":
                    (index,expr) = ReadBody(index,")",true);
                    break;
                case "fun":
                    (index,expr) = ReadFunctionDefinition(index,FunctionType.General);
                    break;
                case "opp":
                    (index,expr) = ReadFunctionDefinition(index,FunctionType.Prefix);
                    break;
                case "opm":
                    (index,expr) = ReadFunctionDefinition(index,FunctionType.Mid);
                    break;
                case "ops":
                    (index,expr) = ReadFunctionDefinition(index,FunctionType.Suffix);
                    break;
                case "\"":
                    string stt;
                    (index,stt) = ReadString(index,'"');
                    expr = new ConstantData<string>(stt);
                    break;
                default:
                    expr = new VariableToken(str);
                    break;
            }

            //直後に(がつく、関数呼び出しかどうかを確認する
            
            // ConstantDataの後ろは必ず関数呼び出しではないので除外
            if (expr is ConstantData<string>)
            {
                exprs.Add(expr);
                continue;
            }
            
            // indexがプログラム範囲内で、直後が(なら関数呼び出し
            while(-1 < index && index < _program.Length && _program[index] == '(')
            {
                TermToken fct;
                // 引数を読み込む
                (index,fct) = ReadBody(index+1,")",true);
                // 関数呼び出しとして保存
                expr = new FunctionCallToken(expr,fct.Expressions);
            }
            
            exprs.Add(expr);
        }

        return !requireClose ? (-1,new TermToken(exprs)) : throw new Exception("body didn't close before EOF.");
    }

    /// <summary>
    /// 文字列を読む
    /// </summary>
    /// <param name="index">開始インデックス</param>
    /// <param name="endSymbol">終了シンボル</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private (int index,string str) ReadString(int index,char endSymbol)
    {
        // index
        if(index == -1 || index >= _program.Length) 
            return (-1,"");

        bool readEscapeSequence = false;
        string token = "";
        
        for (; index < _program.Length; index++)
        {
            var str = _program[index];
            if (readEscapeSequence)
            {
                token += str switch
                {
                    'a' => "\a",
                    'b' => "\b",
                    'f' => "\f",
                    'n' => "\n",
                    'r' => "\r",
                    't' => "\t",
                    'v' => "\v",
                    '\\' => "\\",
                    '\"' => "\"",
                    // エスケープ文字ではないならエラー
                    _ => throw new Exception($"Unknown escape symbol : {str}")
                };
                readEscapeSequence = false;
            }
            else
            {
                // エスケープシーケンスの開始
                if (str == '\\')
                {
                    readEscapeSequence = true;
                    continue;
                }

                // 文字列終了
                if(str == endSymbol){
                    return (index+1,token);
                }
                token += str; // 一文字足す
            }
        }

        throw new Exception("EOF : string not closed.");
    }

    private int SkipSpace(int index)
    {
        if(index == -1 || index >= _program.Length) 
            return -1;
        
        while(index < _program.Length)
        {
            if(_program[index] != ' ' && _program[index] != '\t' && _program[index] != '\n')
            {
                return index;
            }
            index++;
        }
        return -1;
    }

    private (int index,string token) ReadToken(int index,bool skipSpace = true)
    {
        if(skipSpace)
        {
            index = SkipSpace(index);
        }

        if(index == -1) return (-1,"");

        string token = "";
        while(index < _program.Length)
        {
            var s = _program[index];
            if(Symbol.Contains(s))
            {
                return token.Length == 0 ? 
                    (index+1,s.ToString()) : // シンボル
                    (index,token); // トークン
            }
            token += s;
            index++;
        }
        return (-1,token);
    }
}