namespace Grim.VM;

public class RunStack
{
    
    // 現在のスコープ
    public Scope Now { get; private set; } = new(null,null);

    public Scope Push(Scope lexicalScope)
    {
        var newScope = new Scope(lexicalScope, Now);
        Now = newScope;
        return newScope;
    }

    public void Pop()
    {
        Now = Now.DynamicScope ?? throw new Exception("pop count");
    }
    
}