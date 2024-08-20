# Quick Tutorial

## この文書の目的
　Hello Worldモジュール(WANGF/HelloWorldディレクトリと同じもの)を作成し、デスクトップ版で実行するところまでの手順を説明します。

## 前提条件
- WANGFデスクトップ版の本体(BlazorMaui001.exe)が起動すること
- Visual Studio 2022がインストールされ、.NET 8のDLLが作成できること (BlazorやMAUI開発機能は特に要りません。作成するのは一般クラスライブラリのDLLだけです)
- 最低限のVisual StuioとC#に関する知識があること
- 最低限のXMLの知識があれば望ましいが、この段階ではなくても可

## プロジェクトの作成
- Visual Studio 2022を起動する
- 【新しいプロジェクトの作成】を選ぶ
- 上部の検索ボックスに"クラス ライブラリ"と入れる
- ただのクラス ライブラリを選ぶ　(前後に文字が付いた"Razorクラス ライブラリ"などは選ばない)
- プロジェクト名、ソリューション名、場所は自由に選ぶ
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
- Visual StudioのソリューションエクスプローラでAngdRunTime.xmlを右クリックしてプロパティを選ぶ
- ビルドアクションを埋め込みリソースに変更する

## wangflib.dllへの参照の追加
- Visual Studioのソリューションエクスプローラの依存関係を右クリックし、プロジェクト参照の追加などを選び、その後、wangflib.dllへの参照を追加する。wangflib.dllはBlazorMaui001のビルドに成功していればできているはずである。

## モジュールクラスの作成
- プロジェクト作成時に自動的に生成されたClass1.csを右クリックして名前の変更を選び、"(好きな名前).cs"に変更する
- そのファイルの中味を以下のように書き換える

```csharp
using ANGFLib;

namespace HelloWorld
{
    public class HelloWorldModule : ANGFLib.Module
    {
        public override string Id => "{db41d86b-fff7-460e-b682-bb8e7ea3c756}";
    }
    public class HelloWorldModuleEx : ModuleEx
    {
        public override T[] QueryObjects<T>()
        {
            if (typeof(T) == typeof(ANGFLib.Module)) return new T[] { new HelloWorldModule() as T };
            return new T[0];
        }
    }
}

```

　歴史的な経緯により、WANGFには2つのモジュールクラスがある。1つはModuleExを継承した拡張性のあるモジュールで新しいものである。もう1つは、Moduleクラスを継承した基本的な情報を格納する古いものである。

　Moduleクラスを継承したクラスには最低でもIdを実装しなければならない。中味はユニーク文字列であるが、GUIDを使えば確実性が高い。

　ModuleExクラスを継承したクラスは上記の通り変更せず使用して頂きたい。変更して良いのはクラス名だけである。もちろん動作を完全に理解した後なら何をどう書き換えても良い。

## 場所の作成
　WANGFではゲーム開始時にプレイヤーがいる【場所】が必須である。ただし、この場所は【僕の部屋】のような具体的な場所ではなく、【ゲームのエントランス】のような抽象的な場所でも良い。

　そのためには、Placeクラスを継承したクラスを作成する。最低でもHumanReadableNameとIdを実装する。HumanReadableNameは人間可読の名前を返す。文字列はプレイヤーが分かれば何でも良い。Idは任意の文字列である。

```csharp
    public class HelloWorldPlace : ANGFLib.Place
    {
        public override string HumanReadableName => "ハロー・ワールドを言う場所";
        public override string Id => "{551c2659-af9c-4942-b78f-8e5c93bda1da}";
    }
```

　作成したクラスはそのままでは何の効果も発揮しない。Moduleクラスを継承したモジュールのクラスに、GetPlacesメソッドを実装して「そういう場所がある」とシステム側に宣言をする。また、GetStartPlaceメソッドを実装して「初期状態でプレイヤーがいる場所はここだ」とシステム側に宣言する。XMLファイルでstartupModuleに1を宣言したモジュールはGetStartPlaceメソッドを実装しなければならない。

```csharp
    public class HelloWorldModule : ANGFLib.Module
    {
        public override string Id => "{db41d86b-fff7-460e-b682-bb8e7ea3c756}";
        public override string GetStartPlace() => "{551c2659-af9c-4942-b78f-8e5c93bda1da}";
        public override Place[] GetPlaces() => new Place[] { new HelloWorldPlace() };
    }
```

## メニューの作成
　場所に関連付けられたメニューを表示するには、場所クラスでConstructMenuメソッドをオーバーライドする。この中でSimpleMenuItemクラスのインスタンスを作成し、引数のlistにAddする。これで第1引数の文字列をラベルとするボタンが表示されるのでプレイヤーはそれを押すことが出来る。もちろん、何個でも追加できる。ボタンが押されると第2引数のラムダ式が実行される。ここでは、DefaultPersons(システムが提供する基本的なビルドインされた人オブジェクト)の中の「独白」さんに言わせている。「独白」さんは発言者の名前を表示しない特殊な「人」である。Sayメソッドで引数の文字列を画面に表示する。このラムダ式はbool値を返す必要があるので最後にreturn true;と書かれているが値はダミーである。


```csharp
    public class HelloWorldPlace : ANGFLib.Place
    {
        public override string HumanReadableName => "ハロー・ワールドを言う場所";
        public override string Id => "{551c2659-af9c-4942-b78f-8e5c93bda1da}";
        public override bool ConstructMenu(List<SimpleMenuItem> list)
        {
            list.Add(new SimpleMenuItem("Say Hello World", () =>
            {
                DefaultPersons.独白.Say("Hello World");
                return true;
            }));
            return base.ConstructMenu(list);
        }
    }
```

## ビルド
- Visual Studioでビルドを行う
- ビルドが成功すると、プロジェクトのbinディレクトリの何階層か下にDLLファイルが生成される

## ショートカット作成と配置
- DLLファイルを右クリックしてショートカットの作成メニューを探す　(ハードリンク/シンボリックリンクではなくショートカットである)
- DLLへのショートカットをC:\ProgramData\autumn\WANGF\modulesに移動させる

## テスト実行
- WANGF Desktop本体(BlazorMaui001.exe)を起動する (どこのディレクトリからの起動でも良い)
- ゲーム一覧に自分で決めた名前のモジュールが見えることを確認する
- それをクリックする
- Say Hello Worldと書かれたボタンがあり、それを押すとHello Worldと表示されることを確認する

## 配布 (Desktop)
- WANGF Desktop本体(BlazorMaui001.exe)の配布が解決済とする
- 自作DLLのコピー先はどこでも良い
- DLLへのショートカットをC:\ProgramData\autumn\WANGF\modulesに必ず置く

## 配布 (Web)
　詳細手順は長くなるのでここではヒントのみ示す。なお、DLLそのものはDesktopでもWebでも同一である。ただし、Webではファイルを操作できない等の制限は存在する。

- 新しいDLLとしてOnlyEmbeddedModulesを作成する
- そこから自作DLLを参照し、情報提供クラスを作成する。情報提供クラスはOnlyGameStartupInfos.csを参考にして作成する
- ヘッドモジュール(BlazorWA009など)からOnlyEmbeddedModulesを参照する
- これでWebブラウザから参照時に自作DLLの名前がゲームの選択肢に追加表示されるようになる
