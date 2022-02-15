namespace Grim.Token;

public class Tokenizer
{
    
    public const char NameTypePrefix = ':';
    
    public const char DynamicScopePrefix = '@';

    public const string SpaceSymbols = " \t\n\r\f";
    
    public const string Symbol = ";()\"" + " \t\n\r\f";

    private readonly string _program;

    public Tokenizer(string program)
    {
        _program = program;
    }

    public List<IToken> Tokenize()
    {
        var (_,tokens) = ReadBody(0,"end",false);
        return tokens;
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

        List<string> parameterNames = new ();
        if(index < _program.Length)
        {
            if(_program[index] == '(')
            {
                (index,parameterNames) = ReadFunctionParameterDefinition(index+1);
            }
        }
                
        List<IToken> tokens;
        (index,tokens) = ReadBody(index,"end",true);

        return (index,new FunctionToken(type,parameterNames,tokens,priority));
    }

    // スペース区切り
    private (int index,List<string> func) ReadFunctionParameterDefinition(int index)
    {
        var names = new List<string>();

        while(index < _program.Length)
        {
            string token;
            (index,token) = ReadToken(index);

            switch(token)
            {
                case ")":
                    return (index,names);
                case ";":
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
        }

        return (index,names);
    }

    private (int index, List<IToken> terms) ReadBody(int index,string endSymbol,bool requireClose)
    {
        List<IToken> exprs = new ();

        while(index < _program.Length && index != -1)
        {
            string str;
            (index,str) = ReadToken(index);

            if(str == endSymbol)
            {
                return (index,exprs);
            }

            //Console.WriteLine(index + " : " + str);

            IToken expr;
            switch(str)
            {
                case "":
                case " ":
                case "\t":
                case "\n":
                case "\r":
                case "\f":
                    continue;
                case ";":
                    expr = DelimiterToken.Instance;
                    break;
                case ")":
                    throw new Exception("Unexpected close )");
                case "(":
                {
                    List<IToken> innerExprs;
                    (index,innerExprs) = ReadBody(index,")",true);
                    expr = new TermToken(innerExprs);
                    break;
                }
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
                    // TODO 名前型かの確認... でも名前型は評価して作りたいから無理かも
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
                List<IToken> funcBuilder;
                // 引数の式を読み込む
                (index,funcBuilder) = ReadBody(index+1,")",true);
                // 関数呼び出しとして保存
                expr = new FunctionCallToken(expr,funcBuilder);
            }
            
            exprs.Add(expr);
        }

        return !requireClose ? (-1,exprs) : throw new Exception("body didn't close before EOF.");
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
            if(!SpaceSymbols.Contains(_program[index]))
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