# GrimCs
 

[![.NET](https://github.com/nananapo/grim-cs/actions/workflows/dotnet.yml/badge.svg)](https://github.com/nananapo/grim-cs/actions/workflows/dotnet.yml)

[nananapo/grim-lang](https://github.com/nananapo/grim-lang)を良くしたもの

## 概要

工事中

## BNF (作成中)

```
<formula> ::= <formula> <formula> | <term> <delimiter> | <term> <mid-operator> <formula> | <empty>
<term> ::= ( <formula> ) | <prefix-operators> <term> <suffix-operators> | <var> | <value> | <function> | <term> '(' <formula> ')'

<delimiter> ::= ';'

<prefix-operators> ::= <prefix-operator> | <prefix-operator> <prefix-operator>
<suffix-operators> ::= <suffix-operator> | <suffix-operator> <suffix-operator>

<function> ::= <var> | 'fun' '(' <parameters> ')' <formula> <end-scope> | 'fun' <formula> <end-scope> | <builtin-function>
<prefix-operator> ::= <var> | 'opp' <priority> '(' <parameter> ')' <formula> <end-scope>
<suffix-operator> ::= <var> | 'ops' <priority> '(' <parameter> ')'  <formula> <end-scope>
<mid-operator> ::= <var> | 'opm' <priority> '(' <parameter> <parameter> ')'  <formula> <end-scope>

<priority> ::= <int>
<parameter> ::= <string>
<parameters> ::= <parameter> | <parameter> <parameter-list> | <empty>
<end-scope> ::= 'end'

<var> ::= <dynamic-scope-symbol> <string> | <string>

<value> ::= <text> | <int> | <name-type>
<name-type> ::= <name-type-symbol> <string>

<name-type-symbol> ::= ':'
<dynamic-scope-symbol> ::= '@'

<builtin-function> ::= '__assign' | '__add' | '__negate' | '__equal' | '__put' | '__input' 

<text> ::= '"' <string> '"'

<empty> ::= 無
<string> ::= 文字列
<int> ::= 数字列
```
<!--TODO if文 while文 statement -->
