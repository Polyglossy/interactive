using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Microsoft.DotNet.Interactive.CSharpProject.Tests;

internal static class TestEnvironment
{
    // The prebuild tests run `dotnet new` / `dotnet build` in directories outside the repo. By default those are
    // under ~/.trydotnet/prebuilds, where no global.json applies, so the newest installed SDK is selected
    // (e.g. 11.0.x on a runner that has it preinstalled) even though the test host itself runs on the SDK pinned by
    // the repo's global.json. That mismatch makes the tests depend on whichever SDKs a machine happens to have.
    //
    // Point the prebuilds root at a test-owned directory that carries a copy of the repo's SDK pin, so every
    // prebuild resolves the same SDK as the test host. Prebuild reads TRYDOTNET_PREBUILDS_PATH in a static
    // constructor, so this has to run before the first use of Prebuild; a module initializer guarantees that.
    [ModuleInitializer]
    internal static void PinPrebuildsToRepositorySdk()
    {
        // An exception escaping a module initializer fails the whole test host with an unhelpful error, so any
        // problem here falls back to the default prebuilds location instead.
        try
        {
            if (Environment.GetEnvironmentVariable("TRYDOTNET_PREBUILDS_PATH") is not null)
            {
                // An explicit override wins.
                return;
            }

            var repoGlobalJson = FindGlobalJson(AppContext.BaseDirectory);

            if (repoGlobalJson is null)
            {
                return;
            }

            using var document = JsonDocument.Parse(File.ReadAllText(repoGlobalJson));

            if (!document.RootElement.TryGetProperty("sdk", out var sdk))
            {
                return;
            }

            // Only the "sdk" section: the repo's msbuild-sdks (Arcade) must not apply to the generated projects.
            var content = $$"""{ "sdk": {{sdk.GetRawText()}} }""";

            // Keyed by a hash of the pin itself (not just its version) so a stale prebuild created under a
            // different pin is never reused, even if the pin has no version.
            var pinHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)))[..12];

            var prebuildsRoot = Path.Combine(Path.GetTempPath(), "dotnet-interactive-tests", $"prebuilds-{pinHash}");

            Directory.CreateDirectory(prebuildsRoot);

            var globalJsonPath = Path.Combine(prebuildsRoot, "global.json");

            // The directory is keyed by the content, so an existing file is already correct. Skipping the write
            // avoids contention when several test processes start at once.
            if (!File.Exists(globalJsonPath))
            {
                // Write-then-move so another process never observes a partially written file.
                var temporaryPath = Path.Combine(prebuildsRoot, $"global.json.{Guid.NewGuid():N}.tmp");

                File.WriteAllText(temporaryPath, content);

                try
                {
                    File.Move(temporaryPath, globalJsonPath);
                }
                catch (IOException) when (File.Exists(globalJsonPath))
                {
                    // Another process won the race; its file has identical content.
                    File.Delete(temporaryPath);
                }
            }

            Environment.SetEnvironmentVariable("TRYDOTNET_PREBUILDS_PATH", prebuildsRoot);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            // Fall back to the default prebuilds location.
        }
    }

    private static string FindGlobalJson(string startDirectory)
    {
        for (var directory = new DirectoryInfo(startDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "global.json");

            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}
