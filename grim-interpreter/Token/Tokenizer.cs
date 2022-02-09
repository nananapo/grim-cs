using System;
using System.Collections.Generic;

public class Tokenizer
{
    public const string Symbol = "()\" \t\n";

    private readonly string _program;

    public Tokenizer(string program)
    {
        _program = program;
    }

    public TermToken Tokenize()
    {
        var (_,term) = ReadBody(0,"end",false);
        return term;
    }

    private (int,FunctionToken) ReadFunctionDefinition(int index,FunctionType type)
    {

        int priority = -1;
        if(type == FunctionType.Prefix || type == FunctionType.Suffix)
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
                    throw new Exception("Parameter illegal symbol");
            }
            
            if(index == -1)
                throw new Exception("Parameter EOF");

            tokens.Add(new VariableToken(token));
        }

        return (index,tokens);
    }

    private (int index, TermToken terms) ReadBody(int index,string endSymbol,bool requireClose = true)
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
                    expr = new ValueToken(stt);
                    break;
                default:
                    if(-1 < index && index < _program.Length && _program[index] == '(')
                    {
                        TermToken fct;
                        (index,fct) = ReadBody(index+1,")");
                        exprs.Add(new FunctionCallToken(str,fct));
                    }
                    else
                    {
                        expr = new VariableToken(str);
                        exprs.Add(expr);
                    }
                    continue;
            }

            exprs.Add(expr);
        }

        return !requireClose ? (-1,new TermToken(exprs)) : throw new Exception("body didn't close before EOF.");
    }

    private (int index,string str) ReadString(int index,char endSymbol)
    {
        if(index == -1 || index >= _program.Length) return (-1,"");

        string str = "";
        while(index < _program.Length)
        {
            if(_program[index] == endSymbol){
                return (index+1,str);
            }
            str += _program[index];
            index++;
        }
        throw new Exception("String symbol not closed EOF");
    }

    private int SkipSpace(int index)
    {
        if(index == -1 || index >= _program.Length) return -1;
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
                if(token.Length == 0){
                    return (index+1,s.ToString());
                }else{
                    return (index,token); 
                }
            }
            token += s;
            index++;
        }
        return (-1,token);
    }
}