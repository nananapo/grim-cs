﻿namespace Grim.VM;

public sealed class UnknownVariable : IFormula
{
    public readonly string Name;
    
    public UnknownVariable(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return nameof(UnknownVariable) + $"<{Name}>";
    }
}