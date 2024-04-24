md C:\ProgramData\autumn
md C:\ProgramData\autumn\WANGF
md C:\ProgramData\autumn\WANGF\modules
set src=C:\xgh\WANGF\WANGF
set dst=C:\ProgramData\autumn\WANGF\modules
mkshortcut waExtOtsukai.lnk %src%\waExtOtsukai\bin\%debrel%\net8.0\waExtOtsukai.dll
mkshortcut waFirstOtsukai.lnk %src%\waFirstOtsukai\bin\%debrel%\net8.0\waFirstOtsukai.dll
mkshortcut waFirstRPG.lnk %src%\waFirstRPG\bin\%debrel%\net8.0\waFirstRPG.dll
mkshortcut waSimpleRPGBase.lnk %src%\waSimpleRPGBase\bin\%debrel%\net8.0\waSimpleRPGBase.dll
move *.lnk %dst%
pause
