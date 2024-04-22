WANGF Desktop 0.29 Alpha 2023/11/27
===================================

● 概要
　本ファイルはWANGF Desktopのテスト公開版です。
　WANGF DesktopはWANGF (https://wangf.azurewebsites.net/) をWindowsデスクトップで動作するようにしたものです。
　本ファイルは正常に動作することの確認を目的に公開されているもので、動作およびゲーム内容はWeb版とほぼ同等です。(将来的にDesktop版のみの追加機能を実装予定です)
　動作した場合はX(Twitter)で以下のように呟いて頂けると嬉しいです。

@KIRARIN56294043 WANGF Desktop 0.29 Alpha 動作しました

　正常に動作しなかった場合はその旨をお知らせ下さい。

@KIRARIN56294043 WANGF Desktop 0.29 Alpha XXが動作しません

● 動作環境
　Windows 10/11
　(実行ランタイムとして.NET 8が必要です。システムにインストールされていない場合は、初回実行時にダウンロードWebサイトが開きます)

● インストール方法
　任意のフォルダにZIPファイルの内容を展開してください。

● 実行方法
　WANGF Desktopフォルダ内のcreateMauiInstaller.batを実行してください。

● データファイルのWeb版との互換性
　基本的にデータファイルには互換性があります。
　Web版でインポート/エクスポートしたデータファイルはDesktop版でもインポート/エクスポートできます。

● 技術的な詳細
　MAUI Blaorアプリとして作成されています。
　システムの主要部分はRazorライブラリとして作成されており、それをWeb版はBlaorアプリから参照し、Desktop版はMAUI Blaorアプリとして参照しています。

● バグレポート先
X(Twitter) @KIRARIN56294043 同人サークルKIRARI宛メンション
メールアドレス(半角): ｋｉｒａｒｉｎ＠ａｕｔｕｍｎ．ｏｒｇ
