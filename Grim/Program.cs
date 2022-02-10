using grim_interpreter;
using grim_interpreter.Token;
using grim_interpreter.VM;

if(args.Length == 0)
    return;

var fileName = args[0];
var program = string.Join("\n",File.ReadAllLines(fileName));
var tokenizer = new Tokenizer(program);

var term = tokenizer.Tokenize();
var vm = new VirtualMachine();
vm.Execute(term);