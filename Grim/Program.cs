using Grim;
using Grim.Token;
using Grim.VM;

if(args.Length == 0)
    return;

switch (args[0])
{
    case "run":
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: run [program file path]");
            return;
        }
        
        var fileName = args[1];
        var program = string.Join("\n",File.ReadAllLines(fileName));
        
        var tokenizer = new Tokenizer(program);
        var term = tokenizer.Tokenize();
        
        Console.WriteLine(term);
        
        var vm = new VirtualMachine();
        vm.EnableLogging = true;
        vm.Execute(term);
        break;
    case "test":
        
        if (args.Length != 3)
        {
            Console.WriteLine("Usage: test [test folder] [count]");
            return;
        }

        var folderPath = args[1];
        var count = int.Parse(args[2]);

        for (var i = 1; i <= count; i++)
        {
            Test.Assert($"{folderPath}/programs/{i}.grim",
                $"{folderPath}/outputs/{i}.txt",
                $"{folderPath}/inputs/{i}.txt");
        }
        
        break;
}
