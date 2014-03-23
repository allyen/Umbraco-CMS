@ECHO OFF
SET release=7.1.0
SET comment=RC
SET version=%release%

IF [%comment%] EQU [] (SET version=%release%) ELSE (SET version=%release%-%comment%)
ECHO Building Umbraco %version%

ReplaceIISExpressPortNumber.exe ..\src\Umbraco.Web.UI\Umbraco.Web.UI.csproj %release%

ECHO Installing the Microsoft.Bcl.Build package before anything else, otherwise you'd have to run build.cmd twice
SET nuGetFolder=%CD%\..\src\packages\
..\src\.nuget\NuGet.exe install ..\src\Umbraco.Web.UI\packages.config -OutputDirectory %nuGetFolder%

ECHO Performing MSBuild and producing Umbraco binaries, BELLE BUILD EXCLUDED!!! To build belle, Umbraco.Web.UI.csproj must be clean and rebuilt (change in build-debug.proj)
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe "Build-debug.proj" /p:BUILD_RELEASE=%release% /p:BUILD_COMMENT=%comment%

echo off
SET SRC=c:\Repositories\Umbraco\build\_BuildOutput\WebApp
SET DEST=n:\web\webcentrum-dev.muni.cz\%1\web

echo. && echo bin:
robocopy %SRC%\bin %DEST%\bin /S /XO /IS /NJH /NFL /NDL
echo. && echo umbraco:
robocopy %SRC%\umbraco %DEST%\umbraco /S /XO /IS /NJH /NFL /NDL
echo. && echo umbraco_client:
robocopy %SRC%\umbraco_client %DEST%\umbraco_client /S /XO /IS /NJH /NFL /NDL

REM pause