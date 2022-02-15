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

opmの 右結合か左結合の判定はpriorityが正の値か負の値かで行う
右が正,左が負

opp,opsは

現在のスコープに新しく割り当て
__let <- いらん
動的スコープで割り当て(見つからない場合現在のスコープ)
__assign <- できた

基本静的スコープ、@で動的スコープ

関数を呼ぶときは()を強制して、関数の合成ができるようにしたい( ;∀;) <- OK

fun() end() ←これができるようにしたい、でもこれを導入すると引数の指定で,の導入が必要に.... ← なるか？ならなくね？ <-OK

opp end value ops end ← これはできるようにしたい


次はreturnの実装
・returnを指定しないなら最後の式の評価を返す
・式が名前を許容しない限り、式の評価がUnknownVariableで終了することを許さない

Variableの名前を消したい <- OK

関数を定義した場所を静的な定義場所として、静的スコープ？

名前型とUnknownは違う感 <- OK

NameTypeにそれが定義されたスコープを保存させて、割り当て時はそのスコープに割り当てる <- OK

FunctionTokenをラップして、定義されたスコープを保存するクラスを作り、それを実行させる <- OK

PrimitiveFunctionとは???????? Built-inでは??????????? <- OK

;は区切り文字 <- OK
:は名前型 <- OK
@は動的スコープ <- OK

標準出力とエラー出力

文字列

↓

Token列

↓

ExpressionToken列

↓

IFormula列

↓

IVariable

少ない引数で関数呼び出ししたら自動でカリー化

名前を評価する関数
検索して名前型を取得する