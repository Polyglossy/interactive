Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

$thisDir = Split-Path -Parent $PSCommandPath
$toolLocation = ""
$toolVersion = ""

dotnet run --project (Join-Path -Path $thisDir -ChildPath "..\interface-generator") --out-file (Join-Path -Path (Join-Path -Path $thisDir -ChildPath "..\polyglot-notebooks\src") -ChildPath "contracts.ts")

if (Test-Path 'env:DisableArcade') {
     dotnet pack (Join-Path -Path $thisDir -ChildPath "dotnet-interactive.csproj") /p:Version=1.0.0
    $script:toolLocation = Join-Path -Path $thisDir -ChildPath "bin\Release"
    $script:toolVersion = "1.0.0"
} else {
    if ($IsLinux -or $IsMacOS) {
        & "$thisDir/../../build.sh" --pack
    } else {
        & "$thisDir\..\..\build.cmd" -pack
    }

    $script:toolLocation = Join-Path -Path (Join-Path -Path (Join-Path -Path $thisDir -ChildPath "..\..") -ChildPath "artifacts\packages\Debug") -ChildPath "Shipping"
    $script:toolVersion = "1.0.0-dev"
}

if (Get-Command polyglossy-interactive -ErrorAction SilentlyContinue) {
    dotnet tool uninstall -g polyglossy.interactive.tool
}
dotnet tool install -g --ignore-failed-sources --add-source "$toolLocation" --version $toolVersion polyglossy.interactive.tool
