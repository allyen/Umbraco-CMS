CALL build.bat /skipnuget /debug


IF ERRORLEVEL 1 GOTO :showerror
 
IF "%1"=="" GOTO :EOF

echo off
SET SRC=c:\Repositories\Umbraco-CMS\build\_BuildOutput
SET DEST=n:\web\webcentrum2-dev.muni.cz\%1

echo. && echo bin:
robocopy %SRC%\WebApp\bin %DEST%\lib\Umbraco-CMS\src\build\_BuildOutput_Debug\bin /S /IS /NJH /NFL /NDL
REM robocopy %SRC%\UmbracoExamine.PDF %DEST%\lib\Umbraco-CMS\src\build\_BuildOutput_Debug\bin /S /XO /IS /NJH /NFL /NDL
echo. && echo umbraco:
robocopy %SRC%\WebApp\umbraco %DEST%\web\Redakce /S /IS /NJH /NFL /NDL
echo. && echo umbraco_client:
robocopy %SRC%\WebApp\umbraco_client %DEST%\web\Umbraco_Client /S /IS /NJH /NFL /NDL

GOTO :EOF

:showerror
PAUSE