# Quick Tutorial

## この文書の目的
　Hello Worldモジュール(WANGF/HelloWorldディレクトリと同じもの)を作成し、デスクトップ版で実行するところまでの手順を説明します。

## 前提条件
-WANGFデスクトップ版の本体が起動すること
-Visual Studio 2022がインストールされ、.NET 8のDLLが作成できること (BlazorやMAUI開発機能は特に要りません。作成するのは一般クラスライブラリのDLLだけです)
-最低限のVisual StuioとC#に関する知識があること
-最低限のXMLの知識があれば望ましいが、この段階ではなくても可

## プロジェクトの作成
- Visual Studio 2022を起動する
- 【新しいプロジェクトの作成】を選ぶ
- 上部の検索ボックスに"クラス ライブラリ"と入れる
- ただのクラス ライブラリを選ぶ　(前後に文字が付いたRazorクラス ライブラリなどは選ばない)
- プロジェクト名ソリューション名場所は自由に選ぶ
- 【次へ】を押す
- .NET 8.0を選ぶ
- 【作成】を押す

## 静的な情報ファイルの作成
- プロジェクトにAngfRuneTime.xmlという名前でXMLファイルを新規追加する。このファイル名は変更できない。
- AngfRuneTime.xmlを開き以下の内容を入力する

```xml
<?xml version="1.0" encoding="utf-8" ?>
<root xmlns="http://angf.autumn.org/std001">
  <module>HelloWorld.dll</module>
  <startupModule>1</startupModule>
  <name>HelloWorld</name>
  <description>最初の一歩となる最低限のモジュールです</description>
  <id>{c0541918-4b45-4b25-9a6f-09d42cffb978}</id>
</root>
```

- startupModuleの値が1だと、ゲーム本体起動時のメニューに追加される。他のモジュールから参照され、単体ではプレイできないモジュールは0にしておく。
- name要素の値はメニューに表示される名前になるので、分かりやすいように"花子のHelloWorld"などに変更しても良い。添付のHelloWorldと紛らわしいので是非変更しておこう
- description要素の中味はただの解説文なので好きなように書き換えよう
- id要素の値はユニークなID文字列があれば良いので、添付のHelloWorldと確実に区別可能にするために変更しておこう。GUID文字列を使う義務は存在しないが、新しいGUID文字列を生成して使用すると安全性が高い。

## 静的な情報ファイルの埋め込み


## モジュールクラスの作成


## 場所の作成


## メニューの作成

## ビルド

## ショートカット作成と配置


## テスト実行


## 配布



