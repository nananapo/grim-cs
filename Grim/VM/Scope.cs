namespace Grim.VM;

public class Scope
{

    public readonly int ScopeId;
    
    public readonly int LexicalScopeId;

    public readonly int DynamicScopeId; 
        
    private readonly Dictionary<string, IVariable> _dict = new();

    public Scope(int id,int lexicalScopeId, int dynamicScopeId)
    {
        ScopeId = id;
        LexicalScopeId = lexicalScopeId;
        DynamicScopeId = dynamicScopeId;
    }

    public bool TryGet(string name,out IVariable result)
    {
        if (_dict.ContainsKey(name))
        {
            result = _dict[name];
            return true;
        }

        result = Void.Instance;
        return false;
    }

    public void Set(string name,IVariable variable)
    {
        _dict[name] = variable;
    }
}