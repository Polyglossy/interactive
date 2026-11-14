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

function Write-Timestamped([string]$message) {
    $ts = [DateTimeOffset]::UtcNow.ToString("u")
    Write-Host "[$ts] $message"
}

function ExecuteTestDirectory([string]$testDirectory, [string]$extraArgs = "") {
    $projectName = [System.IO.Path]::GetFileName($testDirectory.TrimEnd('/', '\'))
    $blameArgs = "--blame-hang-timeout 10m --blame-hang-dump-type full --blame-crash"

    $testCommand = "dotnet test `"$testDirectory`" $extraArgs -l trx --no-restore --no-build $blameArgs -c $buildConfig --results-directory `"$repoRoot/artifacts/TestResults/$buildConfig`""
    $start = Get-Date
    Write-Timestamped "Starting test command for $projectName"
    Write-Host "Executing $testCommand"
    Invoke-Expression $testCommand
    $elapsed = [Math]::Round(((Get-Date) - $start).TotalSeconds, 2)
    Write-Timestamped "Finished test command for $projectName in ${elapsed}s with exit code $LASTEXITCODE"
}

try {
    $repoRoot = Resolve-Path $PSScriptRoot
    $flakyTestAssemblyDirectories = @(
        "Microsoft.DotNet.Interactive.Tests",
        "Microsoft.DotNet.Interactive.App.Tests",
        "Microsoft.DotNet.Interactive.Browser.Tests"
        )
    
    $normalTestAssemblyDirectories = Get-ChildItem -Path "$repoRoot/src" -Directory -Filter *.Tests -Recurse | Where-Object { !$flakyTestAssemblyDirectories.contains($_.Name)}

    foreach ($testAssemblyDirectory in $normalTestAssemblyDirectories) {
        $projectName = $testAssemblyDirectory.Name
        if($projectsToSkip.contains($projectName)){
            Write-Timestamped "Skipping test project $projectName"
            continue
        }
        for ($i = 1; $i -le $retryCount; $i++) {
            Write-Timestamped "Testing project $projectName, attempt $i"
            ExecuteTestDirectory -testDirectory $testAssemblyDirectory
            if ($LASTEXITCODE -eq 0) {
                Write-Timestamped "Project $projectName succeeded on attempt $i"
                break
            }
            Write-Timestamped "Project $projectName failed on attempt $i with exit code $LASTEXITCODE"
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
            for ($i = 1; $i -le $retryCount; $i++) {
                Write-Timestamped "Testing class $testClass in $flakyTestAssemblyDirectory, attempt $i"
                ExecuteTestDirectory -testDirectory "$repoRoot/src/$flakyTestAssemblyDirectory" -extraArgs "--filter `"FullyQualifiedName~$testClass&Category!=Skip`""
                if ($LASTEXITCODE -eq 0) {
                    Write-Timestamped "Class $testClass in $flakyTestAssemblyDirectory succeeded on attempt $i"
                    break
                }
                Write-Timestamped "Class $testClass in $flakyTestAssemblyDirectory failed on attempt $i with exit code $LASTEXITCODE"
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
