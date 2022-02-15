using Grim.Token;

namespace Grim.VM;

public class RunStack
{
    public int Now { get; private set; } = -1;
    
    public int StackCount { get; private set; } = 0;

    private int SCOPE_ID_INCREMENTAL = 0;

    private readonly Dictionary<int, Scope> _scopes = new()
    {
        {0,new Scope(0,-1, -1)}
    };

    public void Push(int lexicalScopeId)
    {
        StackCount++;
        
        var newScope = new Scope(++SCOPE_ID_INCREMENTAL,lexicalScopeId, Now);
        _scopes[newScope.ScopeId] = newScope;

        Now = newScope.ScopeId;
    }

    public void Pop()
    {
        if (StackCount == 0)
        {
            throw new Exception("pop count");
        }
        StackCount--;

        var scope = _scopes[Now];
        Now = scope.DynamicScopeId;
    }

    public void SetVariable(int scopeId, string name, IVariable variable)
    {
        if (!_scopes.ContainsKey(scopeId))
        {
            throw new Exception($"illegal scope {scopeId}");
        }
        _scopes[scopeId].Set(name,variable);
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
            isLexicalScope = false;
            name = name.Substring(1);
        }
        
        // 遡って探す
        var currentId = Now;
        while (currentId != -1)
        {
            var scope = _scopes[currentId];
            if (scope.TryGet(name,out var result))
            {
                return result;
            }
            currentId = isLexicalScope ? scope.LexicalScopeId : scope.DynamicScopeId;
        }
        
        // 見つからなかったらVoidを返す
        return Void.Instance;
    }
    
}