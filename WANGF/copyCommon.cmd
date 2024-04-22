set src=C:\xgit\WANGF\WANGF\OnlyEmbeddedModules\bin\%debrel%\net8.0
cd %src%
md C:\ProgramData\autumn
md C:\ProgramData\autumn\WANGF
md C:\ProgramData\autumn\WANGF\modules
set dst=C:\ProgramData\autumn\WANGF\modules
mkshortcut waExtOtsukai.lnk %src%\waExtOtsukai.dll
mkshortcut waFirstOtsukai.lnk %src%\waFirstOtsukai.dll
mkshortcut waFirstRPG.lnk %src%\waFirstRPG.dll
mkshortcut waSimpleRPGBase.lnk %src%\waSimpleRPGBase.dll
move *.lnk %dst%
pause
