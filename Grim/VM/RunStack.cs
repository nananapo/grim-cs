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
    public bool TryGetVariable<T>(string name, out T variable) where T : Variable
    {
        variable = GetVariable(name) as T;
        return variable != null;
    }

    // TODO スコープの指定
    public bool TryGetVariable(string name, out Variable variable)
    {
        variable = GetVariable(name);
        return variable is not UnknownVariable;
    }

    // TODO スコープの指定
    public Variable GetVariable(string name)
    {
        foreach (var set in _lexicalScope)
        {
            if (set.Exists(name))
            {
                return set.Get(name);
            }
        }
        return new UnknownVariable(name);
    }

    //TODO スコープの指定
    public void Assign(string name,Variable variable)
    {
        if (name != variable.VariableName)
            throw new ArgumentException("variable.VariableName is not same with name");
        
        foreach (var set in _lexicalScope)
        {
            if (set.Exists(name))
            {
                set.Set(variable);
                return;
            }
        }
        
        _lexicalScope[0].Set(variable);
    }

    public void AssignHere(string name,Variable variable)
    {
        if (name != variable.VariableName)
            throw new ArgumentException("variable.VariableName is not same with name");
        
        _lexicalScope[^1].Set(variable);
    }

    private class VariableSet
    {
        private readonly Dictionary<string, Variable> _dict = new();

        public bool Exists(string name)
        {
            return _dict.ContainsKey(name);
        }

        public Variable Get(string name)
        {
            return Exists(name) ? _dict[name] : new UnknownVariable(name);
        }

        public void Set(Variable variable)
        {
            _dict[variable.VariableName] = variable;
        }
    }
}