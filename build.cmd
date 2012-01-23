@echo Off
set config=%1
if "%config%" == "" (
   set config=debug
)

:: compile the code
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild src/SomeProject.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false

:: remove all obj folder contents
for /D %%f in (".\tests\*") do @(
del /S /Q "%%f\obj\*"
)

:: find all test files and run them
for /R %%F in (*Tests.dll) do (
.\tools\xunit\xunit.console.clr4.exe %%F
)