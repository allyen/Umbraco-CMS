@ECHO OFF
IF NOT EXIST UmbracoVersion.txt (
	ECHO UmbracoVersion.txt missing!
	GOTO :showerror
)

REM Get the version and comment from UmbracoVersion.txt lines 2 and 3
SET "release="
SET "comment="
FOR /F "skip=1 delims=" %%i IN (UmbracoVersion.txt) DO IF NOT DEFINED release SET "release=%%i"
FOR /F "skip=2 delims=" %%i IN (UmbracoVersion.txt) DO IF NOT DEFINED comment SET "comment=%%i"

SET version=%release%

IF [%comment%] EQU [] (SET version=%release%) ELSE (SET version=%release%-%comment%)
ECHO Building Umbraco %version%

ReplaceIISExpressPortNumber.exe ..\src\Umbraco.Web.UI\Umbraco.Web.UI.csproj %release%

ECHO Installing the Microsoft.Bcl.Build package before anything else, otherwise you'd have to run build.cmd twice  
SET nuGetFolder=%CD%\..\src\packages\
..\src\.nuget\NuGet.exe sources Remove -Name MyGetUmbracoCore
..\src\.nuget\NuGet.exe sources Add -Name MyGetUmbracoCore -Source https://www.myget.org/F/umbracocore/api/v2/ >NUL
..\src\.nuget\NuGet.exe install ..\src\Umbraco.Web.UI\packages.config -OutputDirectory %nuGetFolder%
..\src\.nuget\NuGet.exe install ..\src\umbraco.businesslogic\packages.config -OutputDirectory %nuGetFolder%
..\src\.nuget\NuGet.exe install ..\src\Umbraco.Core\packages.config -OutputDirectory %nuGetFolder%

ECHO Removing the belle build folder to make sure everything is clean as a whistle
RD ..\src\Umbraco.Web.UI.Client\build /Q /S

ECHO Removing existing built files to make sure everything is clean as a whistle
RMDIR /Q /S _BuildOutput_Debug
DEL /F /Q UmbracoCms.*.zip
DEL /F /Q UmbracoExamine.*.zip
DEL /F /Q UmbracoCms.*.nupkg
DEL /F /Q webpihash.txt

ECHO Making sure Git is in the path so that the build can succeed
CALL InstallGit.cmd
ECHO Performing MSBuild and producing Umbraco binaries zip files
%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe "Build-debug.proj" /p:BUILD_RELEASE=%release% /p:BUILD_COMMENT=%comment%

IF ERRORLEVEL 1 GOTO :showerror
 
IF "%1"=="" GOTO :EOF

echo off
SET SRC=c:\Repositories\Umbraco-CMS\build\_BuildOutput_Debug
SET DEST=n:\web\webcentrum2-dev.muni.cz\%1

echo. && echo bin:
robocopy %SRC%\WebApp\bin %DEST%\lib\Umbraco-CMS\src\build\_BuildOutput_Debug\bin /S /XO /IS /NJH /NFL /NDL
REM robocopy %SRC%\UmbracoExamine.PDF %DEST%\lib\Umbraco-CMS\src\build\_BuildOutput_Debug\bin /S /XO /IS /NJH /NFL /NDL
echo. && echo umbraco:
robocopy %SRC%\WebApp\umbraco %DEST%\web\Redakce /S /XO /IS /NJH /NFL /NDL
echo. && echo umbraco_client:
robocopy %SRC%\WebApp\umbraco_client %DEST%\web\Umbraco_Client /S /XO /IS /NJH /NFL /NDL

GOTO :EOF

:showerror
PAUSE