Param(
    [Parameter(Mandatory=$True,Position=1)]
    [string]$developer
)

$session = New-PSSession -computername pan

try {
    invoke-command -Session $session -scriptblock { param($developer)
        set-location "c:\w3mu\webcentrum.muni.cz\umbraco-source\Umbraco\build"
        .\Build-debug.bat $developer
        if ($LastExitCode -ne 0) {
            throw "error"
        }
    } -Args $developer
} catch {
    if ($_.Exception.Message -eq "error") {
        Write-Host -ForegroundColor Red "Build failed, press spacebar to rebuild"
    }
}

Exit-PSSession