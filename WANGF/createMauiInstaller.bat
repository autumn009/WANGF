@echo on
call "C:\Program Files\Microsoft Visual Studio\2022\Preview\VC\Auxiliary\Build\vcvars64.bat"

rem ***** アーカイブ作成 *****

rem ランタイム版セットアッププロジェクトをビルド
devenv WANGF.sln /rebuild "DirectKickable" /project BlazorMaui001

del ..\wangf000.zip
set workroot=c:\delme\WANGFMauiWork
rd /s/q %workroot%
md %workroot%
set trash=%workroot%\delme
md %trash%
set work=%workroot%\work
md %work%
set myroot="%work%\WANGF Desktop"
md %myroot%
copy "WANGF Desktop.bat" %myroot%
copy "readme.txt" %myroot%

set mymain=%myroot%\WANGFMaui
md %mymain%
xcopy /s BlazorMaui001\bin\DirectKickable\net8.0-windows10.0.19041.0\win10-x64 %mymain%

azip ..\wangf000.zip %work%
rem 解凍してファイルの妥当性をチェック
azip -d ..\wangf000.zip %trash%


echo 
pause
