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
        
        Console.WriteLine("-------------Parse Result-------------\n" + string.Join(",",term));
        
        var oIndex = 0;
        var iIndex = 0;
        
        var vm = new VirtualMachine(str =>
        {
            foreach (var actual in str.Split("\n"))
            {
                if (oIndex >= outputs.Length)
                    throw new Exception($"Assertion Failed : put call count\n expected : {outputs.Length}\n value : {actual}");

                if (actual != outputs[oIndex])
                    throw new Exception($"Assertion Failed\n expected : {outputs[oIndex]}\n actual : {actual}");

                oIndex++;
            }
        }, () =>
        {
            if (iIndex >= inputs.Length)
                throw new Exception($"Assertion Failed : input call count\n expected : {inputs.Length}");
            return inputs[iIndex++];
        })
        {
            EnableLogging = true
        };

        
        Console.WriteLine("-------------Excursion Result-------------");
        vm.Execute(term);
        
        if(oIndex != outputs.Length)
            throw new Exception($"Assertion Failed : put call count\n expected : {outputs.Length}\n actual : {oIndex}");
        
        if(iIndex != inputs.Length)
            throw new Exception($"Assertion Failed : input call count\n expected : {inputs.Length}\n actual : {iIndex}");
        
        Console.WriteLine($"\nAssertion Succeeded {programFilePath}\n");
    }
}