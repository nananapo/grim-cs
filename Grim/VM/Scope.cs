namespace Grim.VM;

public class Scope
{

    public readonly Scope? LexicalScope;

    public readonly Scope? DynamicScope; 
        
    private readonly Dictionary<string, IVariable> _dict = new();

    public Scope(Scope? lexicalScope, Scope? dynamicScope)
    {
        LexicalScope = lexicalScope;
        DynamicScope = dynamicScope;
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