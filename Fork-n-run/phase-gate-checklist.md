# Phase Gate Checklist

Use this file as the execution record for the rename program. Each phase should capture the exact commands run, the manual notebook scenarios exercised, the rollback point, and the go or hold decision.

## Baseline Command Set

Record the current state before any rename work starts.

Baseline commit recorded for Phase 0 preparation: `efc95f09444d4a55fa291829a3d11d7dbfac7f1a`.

### Root and .NET Baseline

| Command | Expected purpose | Result | Notes |
| --- | --- | --- | --- |
| `./build.cmd` | broad repo build baseline | Passed | PowerShell run as `.\build.cmd`; build succeeded in 53.2s. Baseline output includes npm audit warnings across TS packages, Rollup circular dependency warnings, and SignalR Rollup annotation warnings. |
| `dotnet build src/dotnet-interactive/dotnet-interactive.csproj` | CLI/app slice build | Passed | Build succeeded in 22.8s. Use this instead of the broad root build when isolating Phase 1. |
| `dotnet test src/dotnet-interactive.Tests/dotnet-interactive.Tests.csproj` | core CLI and integration slice | Passed | Test summary: 171 total, 169 succeeded, 2 skipped, 0 failed, duration 85.8s; build succeeded in 98.7s. Skips are existing Jupyter integration and connector reuse tests. |
| `dotnet test src/Microsoft.DotNet.Interactive.Browser.Tests/Microsoft.DotNet.Interactive.Browser.Tests.csproj` | browser and asset-serving slice | Passed | Test summary: 14 total, 14 succeeded, 0 skipped, 0 failed, duration 22.5s; build succeeded in 30.9s. |
| `dotnet test src/Microsoft.DotNet.Interactive.Jupyter.Tests/Microsoft.DotNet.Interactive.Jupyter.Tests.csproj` | Jupyter and kernelspec slice | Passed with skips | Test summary: 199 total, 131 succeeded, 68 skipped, 0 failed, duration 49.3s; build succeeded in 56.2s. Skips require `TEST_DOTNET_JUPYTER_HTTP_CONN` or `TEST_DOTNET_JUPYTER_ZMQ_CONN`. |

### TypeScript Baseline

| Working directory                        | Command           | Expected purpose                     | Result              | Notes                                                                                                                                                                                   |
| ---------------------------------------- | ----------------- | ------------------------------------ | ------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `src/polyglot-notebooks`                 | `npm run compile` | build TS package and library outputs | Passed              | Runs lint, TypeScript build, and Rollup outputs. Baseline Rollup circular dependency warnings reported for `kernel.js` and `kernelInvocationContext.js`.                                |
| `src/polyglot-notebooks`                 | `npm test`        | core package tests                   | Passed with pending | Test summary: 99 passing, 1 pending. Baseline warnings include module type warning and deprecated `punycode` warning.                                                                   |
| `src/polyglot-notebooks-browser`         | `npm run compile` | build browser bundle                 | Passed              | Produces `dist/dotnet-interactive.js`. Baseline warnings include SignalR Rollup pure-annotation warnings, circular dependency warnings, and mixed named/default export warning.         |
| `src/polyglot-notebooks-browser`         | `npm test`        | browser package tests                | Passed              | Test summary: 16 passing. Baseline warnings include module type warning and deprecated `punycode` warning.                                                                              |
| `src/polyglot-notebooks-vscode`          | `npm run compile` | stable extension build               | Passed              | Lint, TypeScript build, preload compile, variable-grid webpack build, and resource copy completed. Baseline Rollup circular dependency warnings reported through the preload compile.   |
| `src/polyglot-notebooks-vscode`          | `npm test`        | stable extension tests               | Passed              | After fixing the shared acquisition test helper, test summary: 137 passing. The previous root tool manifest failure came from a test double that only read `.config\dotnet-tools.json`. |
| `src/polyglot-notebooks-vscode-insiders` | `npm run compile` | insiders extension build             | Passed              | Lint, TypeScript build, preload compile, variable-grid webpack build, and resource copy completed. Baseline Rollup circular dependency warnings reported through the preload compile.   |
| `src/polyglot-notebooks-vscode-insiders` | `npm test`        | insiders extension tests             | Passed              | After fixing the shared acquisition test helper, test summary: 137 passing. This is the same shared test path exercised by the stable extension package.                                |

## Manual Notebook E2E Checklist

Run this sequence after each verification phase.

1. Install or point VS Code at the local extension build.
2. Configure transport and parser settings to the local tool if the current phase changed the command path.
3. Open `NotebookTestScript.dib`.
4. Start the notebook kernel successfully.
5. Execute at least one `C#`, `F#`, `PowerShell`, and `JavaScript` cell.
6. Verify variable sharing across at least two languages.
7. Verify package loading or extension-specific acquisition flow if the phase affected it.
8. Save and reopen the notebook.
9. Verify parser-backed `.dib` and `.ipynb` open/save behavior if the phase touched parser or notebook IDs.
10. Record whether any step still depends on an old name.

Phase 0 manual notebook E2E has been recorded below. Run this checklist again before and after any phase that changes tool paths, notebook IDs, parser IDs, or extension acquisition behavior.

## Phase 0 Preparation Record

- Scope changed: documentation-only preparation record; no rename implementation.
- Compatibility aliases added: none.
- Rollback commit: `efc95f09444d4a55fa291829a3d11d7dbfac7f1a`.
- Proceed or hold: proceed to Phase 1 implementation; automated baselines passed and manual notebook E2E completed successfully.

### Phase 0 Commands Run

- Command: root and .NET baseline command set above
  - Expected purpose: establish broad build, CLI/core, browser, and Jupyter pre-rename state.
  - Result: broad build passed; focused .NET build and test set passed with known skips.
  - Notes: Jupyter integration tests that require live connection strings were skipped because `TEST_DOTNET_JUPYTER_HTTP_CONN` and `TEST_DOTNET_JUPYTER_ZMQ_CONN` were not set.

- Command: TypeScript baseline command set above
  - Expected purpose: establish core package, browser package, stable extension, and insiders extension pre-rename state.
  - Result: compile steps passed; core, browser, stable extension, and insiders extension tests passed after the shared acquisition test helper fix.
  - Notes: baseline warnings include npm audit findings from the root build, Rollup circular dependency warnings, SignalR annotation warnings, Node module type warnings, and `punycode` deprecation warnings.

### Phase 0 Manual E2E Result

- Notebook startup:
  - Result: Completed successfully, used the debug mode to verify.
  - Notes: Verified through the local extension debug flow.

- Multi-language execution:
  - Result: Completed successfully.
  - Notes: Covered C#, F#, PowerShell, and JavaScript cells in `NotebookTestScript.dib`.

- Variable sharing:
  - Result: Completed successfully
  - Notes: Verified sharing across at least two languages.

- Save and reopen:
  - Result: Completed successfully.
  - Notes: Verified existing notebook behavior before persisted ID changes.

- Parser flow:
  - Result: Completed successfully.
  - Notes: Verified parser-backed `.dib` and `.ipynb` open/save behavior before parser or notebook ID changes.

- Jupyter acquisition or kernel registration:
  - Result: Completed successfully.
  - Notes: live Jupyter connection-dependent tests were skipped in automation because required environment variables were not set.

### Phase 0 Stop Conditions Triggered

- None.

## Per-Phase Gate Template

Copy this block for each phase execution.

### Phase X Record

- Scope changed:
- Compatibility aliases added:
- Rollback commit:
- Proceed or hold:

#### Commands Run

- Command:
  - Expected purpose:
  - Result:
  - Notes:

- Command:
  - Expected purpose:
  - Result:
  - Notes:

#### Manual E2E Result

- Notebook startup:
  - Result:
  - Notes:

- Multi-language execution:
  - Result:
  - Notes:

- Variable sharing:
  - Result:
  - Notes:

- Save and reopen:
  - Result:
  - Notes:

- Parser flow:
  - Result:
  - Notes:

- Jupyter acquisition or kernel registration:
  - Result:
  - Notes:

#### Stop Conditions Triggered

- None or list triggered hold criteria from [phased-rename-plan.md](./phased-rename-plan.md).

## Recommended Command Set by Phase

### Phase 1 and Phase 2: .NET Artifact Rename and Verification

- `dotnet build src/dotnet-interactive/dotnet-interactive.csproj`
- `dotnet test src/dotnet-interactive.Tests/dotnet-interactive.Tests.csproj`
- `dotnet test src/Microsoft.DotNet.Interactive.Browser.Tests/Microsoft.DotNet.Interactive.Browser.Tests.csproj`
- `dotnet test src/Microsoft.DotNet.Interactive.Jupyter.Tests/Microsoft.DotNet.Interactive.Jupyter.Tests.csproj`
- run the renamed CLI with `--help`
- run the manual notebook E2E checklist

### Phase 3 and Phase 4: TypeScript Artifact Rename and Verification

- in `src/polyglot-notebooks`: `npm run compile` and `npm test`
- in `src/polyglot-notebooks-browser`: `npm run compile` and `npm test`
- in `src/polyglot-notebooks-vscode`: `npm run compile` and `npm test`
- in `src/polyglot-notebooks-vscode-insiders`: `npm run compile` and `npm test`
- install the locally built VSIX
- run the manual notebook E2E checklist

### Phase 5 and Phase 6: .NET API Rename and Verification

- repeat the focused `.NET` build and test set from Phase 1
- add any interface-generator regeneration and its dependent tests
- rerun browser and Jupyter test projects because string-based type references may move
- run the manual notebook E2E checklist

### Phase 7 and Phase 8: TypeScript API Rename and Verification

- repeat the focused TypeScript compile and test set from Phase 3
- pay special attention to browser package tests for renamed globals and exports
- rerun manual notebook scenarios that exercise JavaScript output and browser APIs

### Phase 9 and Phase 10: Persisted IDs, Docs, and Cleanup

- rerun the manual notebook E2E checklist with an existing notebook and a newly created notebook
- rerun Jupyter-related `.NET` tests
- verify settings migration using old and new config keys
- spot-check docs and samples after cleanup

## Go or Hold Rules

Mark the phase as `hold` immediately if any of the following occurs.

- The local VSIX cannot start a notebook kernel with the local tool.
- The browser asset can no longer be discovered by the .NET host.
- Jupyter registration or parser flow breaks because IDs or executable names drifted.
- A compatibility alias is required but not yet implemented.
- A persisted-state change would strand existing settings or notebooks without fallback.

Do not move to the next phase until the current phase record shows:

- all required commands run
- manual notebook E2E recorded
- rollback commit captured
- explicit `proceed` decision
