# GrimCs
 

[![.NET](https://github.com/nananapo/grim-cs/actions/workflows/dotnet.yml/badge.svg)](https://github.com/nananapo/grim-cs/actions/workflows/dotnet.yml)

[nananapo/grim-lang](https://github.com/nananapo/grim-lang)を良くしたもの

```
__assign(:aisatu "Hello World!")
__put(aisatu)
```

# デモ
http://grim.oekaki.chat

## 概要

この言語の名前はgrimです。

grimには以下の特徴があります。

* デフォルト状態では、一つも演算子が定義されていない
* 前置,中値,後置演算子とその優先度を定義することができる
* 静的スコープと動的スコープを利用できる
* 関数はカリー化される

### 文字列
```
"Line1\nLine2\n\"Line3\""
```
ダブルクォーテーションで囲まれた部分は、文字列として認識されます。

エスケープシーケンスを利用することができ、\\nは改行、\\"は"になります。
文字列の途中で改行した場合でも、改行して文字列が継続されます。

### 関数
```
fun
　処理
end

fun(引数1 引数2)
 プログラム
end
```
関数を定義するには、funキーワードとendキーワードを利用します。

funの直後に括弧を記述すると、引数を受け取るように定義することができます。引数が0個の場合、括弧を省略することができます。

#### ビルトイン関数

grimには、いくつかの関数が最初から定義されています。

| 名前     | 説明                                                         |
| -------- | ------------------------------------------------------------ |
| __put    | 引数1を標準出力する                                          |
| __perror | 引数2を標準エラー出力する                                    |
| __input  | 入力を受け付け、入力された文字列を返す                       |
| __assign | 引数1に名前、引数2に値を受け取り、名前が定義されたスコープの名前に値を割り当てる |
| __add    | 引数1と引数2を足した値を返す                                 |
| __negate | 引数1に数字を受け取り、正負反転する                          |
| __equal  | 引数1と引数2が等しければ1を返し、等しくなければ0を返す       |
| __if     | 引数1が1ならば、引数2に受け取った関数を実行する            |
| __ifElse | __ifの、引数1が1以外なら引数3に受け取った関数を実行するバージョン | 
| __while  | 引数1の関数を評価した結果が1である場合、引数2の関数を実行する。引数1の関数の結果が1以外になるまでこれを繰り返し、最後に引数2の関数の評価を返す。| 
| __read   | 引数1に指定されたファイルの中身を文字列として返す | 
| __eval   | 引数1に渡された文字列をプログラムとして評価して返す | 
| __strAt  | 引数1の文字列の引数2の数字-1番目の文字を返す |

#### 呼び出し

関数を呼び出すには、関数の直後に括弧を記述します。

```
fun(param1)
  __put(param1)
end(1)
```

関数は、上記のように定義と同時に呼び出すことができます。これは、1と表示されます。

```
fun(p1 p2)
  __put(__add(p1 p2))
end(1)(2)
```
関数は自動でカリー化されるため、次のような呼び出しが可能です。

(1)で関数を一つ受け取る関数になり、(2)で関数が実行されます。
#### 戻り値

関数は、最後の式の評価された値を返します。

```
fun 1 end
```

この関数は1を返す関数です。何もプログラムがない場合はVoidという特別な値が返されます。

### 変数

```
__assign(:a 1)
```

上のプログラムでは、aという名前に1を割り当てます。

aの前に:をつけることで、識別子aが定義されているスコープ情報と識別子の名前を持つ\"名前型\"を生成できます。

__assignは、第一引数に名前型を受け取り、スコープ情報と識別子からその名前に値を割り当てます。

変数には数字や文字列だけでなく、関数や名前型も割り当てることができます。

#### スコープ

grimは基本的に静的スコープですが、変数の先頭に@をつけることで、動的スコープで変数を参照することができます。

### 演算子

```
opp 優先度(引数1)
  プログラム
end

ops 優先度(引数1)
  プログラム
end

opm 優先度(引数1 引数2)
  プログラム
end
```

oppで前置演算子、opsで後置演算子、opmで中値演算子を定義することができます。

#### 式の評価

項を
```
前置演算子 前置演算子 前置演算子 ... 前置演算子 値 後置演算子 後置演算子 後置演算子 ...  後置演算子
```
のような形で定義されるものとします。

式を
```
項 中値演算子 項 中値演算子 項 ... 項 中値演算子 項
```
のような形で定義されるものとします。

プログラムはすべて式で構成されています。
grimでは、次の順序で式を処理(評価)します。
```
括弧内の式 > 項の前置演算子と後置演算子(優先度順) > 中値演算子(優先度順)
```

#### 前置演算子と後置演算子の評価順

前置演算子と後置演算子は、項の中間に値に最も近い前置演算子と後置演算子から処理していきます。前置演算子と後置演算子の優先度を比較し、大きいほうを処理したら、比較、処理を繰り返します。

#### 中値演算子の処理

中値演算子は優先度の絶対値が大きい順に処理されます。その際、優先度の絶対値が同じ演算子の優先度が、すべて正の値なら右結合、負の値なら左結合になります。
正負が混ざっている場合、左結合になります。

```
__assign(:=; opm 1(name value) __assign(name; value) end)
:+  = opm -5(va1 va2) __add(va1; va2) end
:-  = opm -5(va1 va2) va1 + __negate(va2) end
:*  = opm -7(va1 va2) __ifElse(va2 == 0 fun 0 end fun va1 + va1 * (va2 - 1) end) end;
:== = opm 3(va1 va2) __equal(va1; va2) end

:a = 1 + 3 * 2
__put(a)
```

上のプログラムの場合、3 \* 2 , 1 + 6 , \:a = 7 の順に実行されます。

## サンプル

あると便利な演算子と関数の実装

https://github.com/nananapo/grim-cs/blob/main/Grim/std.grim

スタックを操作する関数を返す関数

https://github.com/nananapo/grim-cs/blob/main/Grim/list.grim
