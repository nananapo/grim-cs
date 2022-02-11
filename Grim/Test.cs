namespace Grim;

using Token;
using VM;

public static class Test
{
    public static void Assert(string programFilePath,string outputFilePath,string? inputFilePath = null)
    {
        var program = string.Join("\n",File.ReadAllLines(programFilePath));
        var outputs = File.ReadAllLines(outputFilePath);
        var inputs = inputFilePath != null ? File.ReadAllLines(inputFilePath) : Array.Empty<string>();

        var tokenizer = new Tokenizer(program);
        var term = tokenizer.Tokenize();
        
        var oIndex = 0;
        var iIndex = 0;
        
        var vm = new VirtualMachine(actual =>
        {
            // TODO 改行を考慮
            if (oIndex >= outputs.Length)
                throw new Exception($"Assertion Failed : put call count\n expected : {outputs.Length}\n value : {actual}");

            if (actual != outputs[oIndex])
                throw new Exception($"Assertion Failed\n expected : {outputs[oIndex]}\n actual : {actual}");

            oIndex++;
        }, () =>
        {
            // TODO 改行を考慮
            if (iIndex >= inputs.Length)
                throw new Exception($"Assertion Failed : input call count\n expected : {inputs.Length}");
            return inputs[iIndex++];
        });
        
        vm.Execute(term);
        
        if(oIndex != outputs.Length)
            throw new Exception($"Assertion Failed : put call count\n expected : {outputs.Length}\n actual : {oIndex}");
        
        if(iIndex != inputs.Length)
            throw new Exception($"Assertion Failed : input call count\n expected : {inputs.Length}\n actual : {iIndex}");
        
        Console.WriteLine($"Assertion Succeeded {programFilePath}");
    }
}