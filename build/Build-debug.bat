@ECHO OFF
SET release=6.1.6
SET comment=
SET version=%release%

IF [%comment%] EQU [] (SET version=%release%) ELSE (SET version=%release%-%comment%)

ReplaceIISExpressPortNumber.exe ..\src\Umbraco.Web.UI\Umbraco.Web.UI.csproj %release%

%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe "Build-debug.proj" /p:BUILD_RELEASE=%release% /p:BUILD_COMMENT=%comment%

echo off
SET SRC=N:\web\webcentrum-dev.muni.cz\umbraco-source\Umbraco\build\_BuildOutput\WebApp
SET DEST=N:\web\webcentrum-dev.muni.cz\%1\web

echo. && echo bin:
robocopy %SRC%\bin %DEST%\bin /S /XO /NJH /NFL /NDL
echo. && echo umbraco:
robocopy %SRC%\umbraco %DEST%\umbraco /S /XO /NJH /NFL /NDL
echo. && echo umbraco_client:
robocopy %SRC%\umbraco_client %DEST%\umbraco_client /S /XO /NJH /NFL /NDL

REM pause