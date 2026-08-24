[CmdletBinding(PositionalBinding = $false)]
param (
    [string]$stableToolVersionNumber,
    [string]$outDir
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

function Get-VersionWithIncrementedPatch([string]$versionNumber, [int]$patchIncrement = 0) {
    if ($versionNumber -notmatch '^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?<suffix>(?:[-+].*)?)$') {
        throw "Unsupported extension version format '$versionNumber'."
    }

    $patch = [int]$Matches['patch'] + $patchIncrement
    return "$($Matches['major']).$($Matches['minor']).$patch$($Matches['suffix'])"
}

function Invoke-ExternalCommand([scriptblock]$command, [string]$errorMessage) {
    & $command
    if ($LASTEXITCODE -ne 0) {
        throw "$errorMessage Exit code: $LASTEXITCODE"
    }
}

function Build-VsCodeExtension([string] $packageDirectory, [string] $outputSubDirectory, [string] $packageVersionNumber, [string] $kernelVersionNumber = "", [bool] $isPrerelease = $false) {
    Push-Location $packageDirectory

    $packageJsonPath = Join-Path (Get-Location) "package.json"
    $originalPackageJsonBytes = [System.IO.File]::ReadAllBytes($packageJsonPath)
    $packageJsonContents = ReadJson -packageJsonPath $packageJsonPath

    try {
        SetNpmVersionNumber -packageJsonContents $packageJsonContents -packageVersionNumber $packageVersionNumber

        # set tool version
        if ($kernelVersionNumber -Ne "") {
            Write-Host "Setting tool version to $kernelVersionNumber"
            $packageJsonContents.contributes.configuration.properties."dotnet-interactive.requiredInteractiveToolVersion"."default" = $kernelVersionNumber
        }

        SaveJson -packageJsonPath $packagejsonPath -packageJsonContents $packageJsonContents

        # create destination
        if ($outputSubDirectory -Eq "") {
            $outputSubDirectory = $packageDirectory
        }
        EnsureCleanDirectory -location "$outDir\$outputSubDirectory"

        $vsixPath = "$outDir\$outputSubDirectory\dotnet-interactive-vscode-$packageVersionNumber.vsix"
        $manifestPath = "$outDir\$outputSubDirectory\dotnet-interactive-vscode-$packageVersionNumber.manifest"

        $packageArgs = @('@vscode/vsce', 'package', '-o', $vsixPath)
        if ($isPrerelease) {
            $packageArgs += '--pre-release'
        }

        Write-Host "Packing extension"
        Invoke-ExternalCommand -command { npx @packageArgs } -errorMessage 'VS Code extension packaging failed.'

        Write-Host "Generating extension manifest"
        Invoke-ExternalCommand -command { npx @vscode/vsce generate-manifest -i $vsixPath -o $manifestPath } -errorMessage 'VS Code extension manifest generation failed.'

        Write-Host "Preparing manifest for signing"
        Copy-Item -Path $manifestPath -Destination "$outDir\$outputSubDirectory\dotnet-interactive-vscode-$packageVersionNumber.signature.p7s"
    }
    finally {
        [System.IO.File]::WriteAllBytes($packageJsonPath, $originalPackageJsonBytes)
        Pop-Location
    }
}

try {
    . "$PSScriptRoot\PackUtilities.ps1"

    # copy publish scripts
    EnsureCleanDirectory -location $outDir
    Copy-Item -Path $PSScriptRoot\..\publish\* -Destination $outDir -Recurse

    $stablePackageVersion = Get-VersionWithIncrementedPatch -versionNumber $stableToolVersionNumber
    $insidersPackageVersion = Get-VersionWithIncrementedPatch -versionNumber $stableToolVersionNumber -patchIncrement 1
    Build-VsCodeExtension -packageDirectory "polyglot-notebooks-vscode" -outputSubDirectory "stable-locked" -packageVersionNumber $stablePackageVersion 
    Build-VsCodeExtension -packageDirectory "polyglot-notebooks-vscode" -outputSubDirectory "stable" -packageVersionNumber $stablePackageVersion -kernelVersionNumber $stableToolVersionNumber
    Build-VsCodeExtension -packageDirectory "polyglot-notebooks-vscode-insiders" -outputSubDirectory "insiders" -packageVersionNumber $insidersPackageVersion -kernelVersionNumber $stableToolVersionNumber -isPrerelease $true
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}
