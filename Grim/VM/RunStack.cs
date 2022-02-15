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
# if DEBUG
        // TODO GCしたら消えるかも
        if (!_scopes.ContainsKey(scopeId))
        {
            throw new Exception($"illegal scope {scopeId}");
        }
#endif
        _scopes[scopeId].Variables[name] = variable;
    }

    /// <summary>
    /// 変数を取得する
    /// </summary>
    /// <param name="name"></param>
    /// <returns>結果、見つからなかったらVoid</returns>
    /// <exception cref="Exception"></exception>
    public bool TryGetVariable(string name,out IVariable result)
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
            
            // 見つけたら返す
            if (scope.Variables.ContainsKey(name))
            {
                result = scope.Variables[name];
                return true;
            }
            
            currentId = isLexicalScope ? scope.LexicalScopeId : scope.DynamicScopeId;
        }
        
        // 見つからなかったらVoidを返す
        result = Void.Instance;
        return false;
    }
    
    private class Scope
    {

        public readonly int ScopeId;
    
        public readonly int LexicalScopeId;

        public readonly int DynamicScopeId; 
        
        public readonly Dictionary<string, IVariable> Variables = new();

        public Scope(int id,int lexicalScopeId, int dynamicScopeId)
        {
            ScopeId = id;
            LexicalScopeId = lexicalScopeId;
            DynamicScopeId = dynamicScopeId;
        }
    }
}
