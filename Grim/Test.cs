namespace Grim;

using Token;
using VM;

public static class Test
{
    private static void Log(string text)
    {
        Console.WriteLine($"[Test] {text}");
    }
    
    public static void Assert(string programFilePath,string outputFilePath,string inputFilePath)
    {
        var program = string.Join("\n",File.ReadAllLines(programFilePath));

        string[] inputs;
        if (File.Exists(inputFilePath))
        {
            inputs = File.ReadAllLines(inputFilePath);
        }
        else
        {
            inputs = Array.Empty<string>();
            Log($"input file for {programFilePath} is not found.");
        }
        
        string[] outputs;
        if (File.Exists(outputFilePath))
        {
            outputs = File.ReadAllLines(outputFilePath);
        }
        else
        {
            outputs = Array.Empty<string>();
            Log($"output file for {programFilePath} is not found.");
        }

        var tokenizer = new Tokenizer(program);
        var term = tokenizer.Tokenize();
        
        Log("-------------Parse Result-------------\n" + string.Join(",",term));
        
        var oIndex = 0;
        var iIndex = 0;

        var vm = new VirtualMachine(str =>
        {
            foreach (var actual in str.Split("\n"))
            {
                if (oIndex >= outputs.Length)
                    throw new Exception(
                        $"Assertion Failed : put call count\n expected : {outputs.Length}\n value : {actual}");

                if (actual != outputs[oIndex])
                    throw new Exception($"Assertion Failed\n expected : {outputs[oIndex]}\n actual : {actual}");

                oIndex++;
            }
        }, () =>
        {
            if (iIndex >= inputs.Length)
                throw new Exception($"Assertion Failed : input call count\n expected : {inputs.Length}");
            return inputs[iIndex++];
        },enableLogging:true);

        
        Log("-------------Excursion Result-------------");
        vm.Execute(term);
        
        if(oIndex != outputs.Length)
            throw new Exception($"Assertion Failed : put call count\n expected : {outputs.Length}\n actual : {oIndex}");
        
        if(iIndex != inputs.Length)
            throw new Exception($"Assertion Failed : input call count\n expected : {inputs.Length}\n actual : {iIndex}");
        
        Log($"Assertion Succeeded {programFilePath}\n");
    }
}