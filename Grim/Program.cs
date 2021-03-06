using System.Text.RegularExpressions;
using Grim;
using Grim.Token;
using Grim.VM;

if(args.Length == 0)
    return;

switch (args[0])
{
    case "run":
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: run [program file path]");
            return;
        }
        var fileName = args[1];
        var program = string.Join("\n",File.ReadAllLines(fileName));
        
        var tokenizer = new Tokenizer(program);
        var term = tokenizer.Tokenize();

        var vm = new VirtualMachine();
        vm.Execute(term);
        break;
    }
    case "test":
        
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: test [test folder]");
            return;
        }

        var origin = args[1] + Path.DirectorySeparatorChar;

        var programFolder = origin + "programs" + Path.DirectorySeparatorChar;
        var outputsFolder = origin + "outputs" + Path.DirectorySeparatorChar;
        var inputsFolder  = origin + "inputs" + Path.DirectorySeparatorChar;

        foreach (var filename in Directory.GetFiles(programFolder,"*.grim"))
        {
            var match = Regex.Match(filename.Substring(programFolder.Length-1-1),"[^/](.*?)\\.grim$");
            
            if (match.Groups.Count < 2)
            {
                continue;
            }

            var name = match.Groups.Values.ToList()[1].Captures.ToList()[0].Value;
            
            Test.Assert($"{programFolder}{name}.grim",
                $"{outputsFolder}{name}.txt",
                $"{inputsFolder}{name}.txt");
        }

        break;
}
