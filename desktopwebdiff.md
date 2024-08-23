# Desktop版とWeb版の相違について

- DesktopはMAUI Blazorの技術で作られていてローカルの.NETで実行されます。WebはBlazor WebAssemblyの技術で作られていてWASMで実行されます。
- DesktopはWindows 10/11で動作します。WebはWASM対応のWebブラウザ(Chrome, Edge, Firefox, Safariなど)で動作します。
- Desktopは利用者が利用するモジュールをきめ細かく追加削除できます。既存のゲームを拡張するモジュールを追加することも出来ます。Webでは決め打ちのモジュールしかプレイできません。
- Desktopはネットワーク接続なしで動作します。Webはネットワークがないと動作しません。
- Desktopは、特にネットに送信する機能を使わない限り、プレイの秘密は守られます。Webはサーバ側に利用の履歴が残ります。
- Desktopは、C:\ProgramData\autumn\WANGF\modulesにあるショートカットで読み込むモジュールを決定します。ショートカットは自分で追加削除できます。Webは埋め込まれた内藏モジュールのみを読み込めます。



