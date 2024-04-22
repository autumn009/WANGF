md C:\ProgramData\autumn
md C:\ProgramData\autumn\WANGF
md C:\ProgramData\autumn\WANGF\modules
set dst=C:\ProgramData\autumn\WANGF\modules
mkshortcut waExtOtsukai.lnk waExtOtsukai\bin\%debrel%\net8.0\waExtOtsukai.dll
mkshortcut waFirstOtsukai.lnk waFirstOtsukai\bin\%debrel%\net8.0\waFirstOtsukai.dll
mkshortcut waFirstRPG.lnk waFirstRPG\bin\%debrel%\net8.0\waFirstRPG.dll
mkshortcut waSimpleRPGBase.lnk waSimpleRPGBase\bub\%debrel%\net8.0\waSimpleRPGBase.dll
move *.lnk %dst%
pause
