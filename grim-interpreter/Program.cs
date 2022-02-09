if(args.Length == 0)
    return;

var fileName = args[0];
var program = string.Join("\n",File.ReadAllLines(fileName));
var tokenizer = new Tokenizer(program);

Console.WriteLine("--Token--");
var term = tokenizer.Tokenize();
Console.WriteLine(term);

var vm = new VirtualMachine();
vm.Execute(term);