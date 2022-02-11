namespace Grim.VM;

public class RunStack
{
    
    private readonly List<VariableSet> _lexicalScope = new();
    //TODO 後で実装
    //private readonly List<VariableSet> _dynamicScope = new();

    public void Push()
    {
        _lexicalScope.Add(new VariableSet());
    }

    public void Pop()
    {
        _lexicalScope.RemoveAt(_lexicalScope.Count-1);
    }
    
    // TODO スコープの指定
    public bool TryGetVariable<T>(string name, out T variable) where T : IVariable
    {
        variable = GetVariable(name) as T;
        return variable != null;
    }

    // TODO スコープの指定
    public bool TryGetVariable(string name, out IVariable variable)
    {
        variable = GetVariable(name);
        return variable is not Void;
    }

    public IVariable GetVariable(string name)
    {
        foreach (var set in _lexicalScope)
        {
            if (set.Exists(name))
            {
                return set.Get(name);
            }
        }
        return Void.Create();
    }

    public void Assign(string name,IVariable variable)
    {
        foreach (var set in _lexicalScope)
        {
            if (set.Exists(name))
            {
                set.Set(name,variable);
                return;
            }
        }
        _lexicalScope[0].Set(name,variable);
    }

    public void AssignHere(string name,IVariable variable)
    {
        _lexicalScope[^1].Set(name,variable);
    }

    private class VariableSet
    {
        private readonly Dictionary<string, IVariable> _dict = new();

        public bool Exists(string name)
        {
            return _dict.ContainsKey(name);
        }

        public IVariable Get(string name)
        {
            return Exists(name) ? _dict[name] : Void.Create();
        }

        public void Set(string name,IVariable variable)
        {
            _dict[name] = variable;
        }
    }
}