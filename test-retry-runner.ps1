[CmdletBinding(PositionalBinding = $false)]
param (
    [int]$retryCount = 5,
    [string]$buildConfig = "Debug"
)

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

if ($IsWindows) {
    $projectsToSkip = @(
    )
}
else
{
    $projectsToSkip = @(
        "Microsoft.DotNet.Interactive.NetFramework.Tests",
        "Microsoft.DotNet.Interactive.NamedPipeConnector.Tests",
        "Microsoft.DotNet.Interactive.VisualStudio.Tests"
    )
}

function Get-SafeFilePart([string]$value) {
    if ([string]::IsNullOrWhiteSpace($value)) { return "unknown" }
    # Replace characters invalid in filenames and trim long names
    $safe = ($value -replace '[<>:"/\\|?*\x00-\x1F]', '_')
    if ($safe.Length -gt 120) { $safe = $safe.Substring(0,120) }
    return $safe
}

function ExecuteTestDirectory([string]$testDirectory, [string]$extraArgs = "", [string]$trxFileName) {
    $resultsDir = Join-Path $repoRoot "artifacts/TestResults/$buildConfig"
    New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null

    $trxArg = "--logger `"trx;LogFileName=$trxFileName`""
    $testCommand = "dotnet test `"$testDirectory/`" $extraArgs $trxArg --no-restore --no-build --blame-hang-timeout 10m --blame-hang-dump-type full --blame-crash -c $buildConfig --results-directory `"$resultsDir`""
    Write-Host "Executing $testCommand"
    Invoke-Expression $testCommand
}

try {
    $repoRoot = Resolve-Path $PSScriptRoot
    $flakyTestAssemblyDirectories = @(
        "Microsoft.DotNet.Interactive.Tests",
        "Microsoft.DotNet.Interactive.App.Tests",
        "Microsoft.DotNet.Interactive.Browser.Tests"
    )

    $normalTestAssemblyDirectories = Get-ChildItem -Path "$repoRoot/src" -Directory -Filter *.Tests -Recurse | Where-Object { !$flakyTestAssemblyDirectories.contains($_.Name) }

    foreach ($testAssemblyDirectory in $normalTestAssemblyDirectories) {
        $projectName = $testAssemblyDirectory.Name
        if($projectsToSkip.contains($projectName)){
            Write-Host "Skipping test project $projectName"
            continue
        }
        for ($i = 1; $i -le $retryCount; $i++) {
            $trxFile = "{0}.attempt{1}.trx" -f (Get-SafeFilePart $projectName), $i
            Write-Host "Testing project $projectName, attempt $i (TRX: $trxFile)"
            ExecuteTestDirectory -testDirectory $testAssemblyDirectory.FullName -trxFileName $trxFile
            if ($LASTEXITCODE -eq 0) {
                break
            }
        }
        if ($LASTEXITCODE -ne 0) {
            exit $LASTEXITCODE
        }
    }

    foreach ($flakyTestAssemblyDirectory in $flakyTestAssemblyDirectories){
        $testNamePattern = "    ([^(]+)" # skip 4 spaces then get everything that's not a left paren because test names start with 4 spaces and [Theory] tests have a parenthesized argument list
        $testNames = dotnet test "$repoRoot/src/$flakyTestAssemblyDirectory/" --no-restore --no-build --configuration $buildConfig --list-tests | Select-String -Pattern $testNamePattern | ForEach-Object { $_.Matches[0].Groups[1].Value }
        $testClasses = $testNames | ForEach-Object { $_.Substring(0, $_.LastIndexOf([char]".")) } # trim off the test name, just get the class
        $distinctTestClasses = $testClasses | Get-Unique

        foreach ($testClass in $distinctTestClasses) {
            $safeClass = Get-SafeFilePart $testClass
            for ($i = 1; $i -le $retryCount; $i++) {
                $trxFile = "{0}.{1}.attempt{2}.trx" -f (Get-SafeFilePart $flakyTestAssemblyDirectory), $safeClass, $i
                Write-Host "Testing class $testClass, attempt $i (TRX: $trxFile)"
                ExecuteTestDirectory -testDirectory "$repoRoot/src/$flakyTestAssemblyDirectory" -extraArgs "--filter `"FullyQualifiedName~$testClass&Category!=Skip`"" -trxFileName $trxFile
                if ($LASTEXITCODE -eq 0) {
                    break
                }
            }
            if ($LASTEXITCODE -ne 0) {
                exit $LASTEXITCODE
            }
        }
    }
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    Write-Host $_.ScriptStackTrace
    exit 1
}