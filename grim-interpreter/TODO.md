関数の定義を全て無名関数の_assignにする
_assign fun(params) term end 

builtin functions

assign name value
input(text)
put

fun1(a b)
fun2(a b) -> a

fun1 fun2 a b b : OK
fun1 (fun2 a b) b


Variableは変数、数字の順


Term := 前置演算子 前置演算子 前置演算子 ... 前置演算子 Expression 後置演算子 ... 後置演算子 後置演算子
Expression := Term 二項演算子 Term

Termを解析
↓
Expressionから一つの式を取り出し
↓
Termを評価

fun() end
opp rank () end
opm rank () end
ops rank () end

右結合か左結合の判定はpriorityが正の値か負の値かで行う