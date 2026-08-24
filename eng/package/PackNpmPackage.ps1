[CmdletBinding(PositionalBinding = $false)]
param (
    [string]$packageVersionNumber,
    [string]$outDir
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

function Invoke-ExternalCommand([scriptblock]$command, [string]$errorMessage) {
    $output = & $command
    if ($LASTEXITCODE -ne 0) {
        throw "$errorMessage Exit code: $LASTEXITCODE"
    }

    return $output
}

function Build-NpmPackage() {
    $packageJsonPath = Join-Path (Get-Location) "package.json"
    $originalPackageJsonBytes = [System.IO.File]::ReadAllBytes($packageJsonPath)
    $packageJsonContents = ReadJson -packageJsonPath $packageJsonPath

    try {
        SetNpmVersionNumber -packageJsonContents $packageJsonContents -packageVersionNumber $packageVersionNumber
        SaveJson -packageJsonPath $packagejsonPath -packageJsonContents $packageJsonContents

        Write-Host "Packing package"
        $packOutput = Invoke-ExternalCommand -command { npm pack } -errorMessage 'NPM package creation failed.'
        $tarballName = $packOutput | Select-Object -Last 1
        if ([string]::IsNullOrWhiteSpace($tarballName)) {
            throw 'NPM package creation did not report an output tarball name.'
        }

        Copy-Item -Path (Join-Path (Get-Location) $tarballName.Trim()) -Destination $outDir
    }
    finally {
        [System.IO.File]::WriteAllBytes($packageJsonPath, $originalPackageJsonBytes)
    }
}

try {
    . "$PSScriptRoot\PackUtilities.ps1"

    # copy publish scripts
    EnsureCleanDirectory -location $outDir
    Copy-Item -Path $PSScriptRoot\..\publish\* -Destination $outDir -Recurse

    Build-NpmPackage
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
