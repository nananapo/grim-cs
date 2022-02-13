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

    public bool Exists(string name)
    {
        return _dict.ContainsKey(name);
    }

    public IVariable Get(string name)
    {
        return _dict.GetValueOrDefault(name,Void.Create());
    }

    public void Set(string name,IVariable variable)
    {
        _dict[name] = variable;
    }
}