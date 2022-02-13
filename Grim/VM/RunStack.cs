using Grim.Token;

namespace Grim.VM;

public class RunStack
{

    public readonly Scope Root = new(null,null);
    
    // 現在のスコープ
    public Scope Now { get; private set; }

    public int StackCount { get; private set; } = 0;

    public RunStack()
    {
        Now = Root;
    }

    public Scope Push(Scope lexicalScope)
    {
        StackCount++;
        var newScope = new Scope(lexicalScope, Now);
        Now = newScope;
        return newScope;
    }

    public void Pop()
    {
        StackCount--;
        Now = Now.DynamicScope ?? throw new Exception("pop count");
    }

    /// <summary>
    /// 変数を取得する
    /// </summary>
    /// <param name="name"></param>
    /// <returns>結果、見つからなかったらVoid</returns>
    /// <exception cref="Exception"></exception>
    public IVariable GetVariable(string name)
    {
        bool isLexicalScope = true;
        
        // 動的スコープか静的スコープかの判定
        // TODO Tokenizerでどうにかしたい
        if (name[0] == Tokenizer.DynamicScopePrefix)
        {
            if (name.Length == 1)
            {
                throw new Exception("@の後には識別子が必要です");
            }
            isLexicalScope = true;
            name = name.Substring(1);
        }
        
        // 遡って探す
        var current = Now;
        while (current != null)
        {
            if (current.TryGet(name,out var result))
            {
                return result;
            }
            current = isLexicalScope ? current.LexicalScope : current.DynamicScope;
        }
        
        // 見つからなかったらVoidを返す
        return Void.Instance;
    }
    
}