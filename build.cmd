@echo Off

set config=%1
if "%config%" == "" (
   set config=Debug
)

set version=%2
if "%version%" == "" (
   set version="0.0.0.0"
)

%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild build.proj /p:Configuration="%config%";Version="%version%" /t:package /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
