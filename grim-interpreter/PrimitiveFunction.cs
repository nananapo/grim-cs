using System.Collections.Generic;

public class PrimitiveFunction
{

    public readonly PrimitiveFunctionType Type;

    public readonly List<VariableToken> Parameters;

    public PrimitiveFunction(PrimitiveFunctionType type,List<VariableToken> parameters)
    {
        Type = type;
        Parameters = parameters;
    }
}