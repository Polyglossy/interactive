# Phase Gate Checklist

Use this file as the execution record for the rename program. Each phase should capture the exact commands run, the manual notebook scenarios exercised, the rollback point, and the go or hold decision.

## Baseline Command Set

Record the current state before any rename work starts.

### Root and .NET Baseline

| Command | Expected purpose | Result | Notes |
| --- | --- | --- | --- |
| `./build.cmd` | broad repo build baseline | not yet recorded | Current terminal context shows a previous failure; treat it as baseline input, not a rename regression. |
| `dotnet build src/dotnet-interactive/dotnet-interactive.csproj` | CLI/app slice build | not yet recorded | Use this instead of the broad root build when isolating Phase 1. |
| `dotnet test src/dotnet-interactive.Tests/dotnet-interactive.Tests.csproj` | core CLI and integration slice | not yet recorded | Run before and after .NET rename waves. |
| `dotnet test src/Microsoft.DotNet.Interactive.Browser.Tests/Microsoft.DotNet.Interactive.Browser.Tests.csproj` | browser and asset-serving slice | not yet recorded | Required for phases that touch browser bundle discovery. |
| `dotnet test src/Microsoft.DotNet.Interactive.Jupyter.Tests/Microsoft.DotNet.Interactive.Jupyter.Tests.csproj` | Jupyter and kernelspec slice | not yet recorded | Required for persisted ID migration phases. |

### TypeScript Baseline

| Working directory | Command | Expected purpose | Result | Notes |
| --- | --- | --- | --- | --- |
| `src/polyglot-notebooks` | `npm run compile` | build TS package and library outputs | not yet recorded | Runs lint plus TypeScript build and rollup outputs. |
| `src/polyglot-notebooks` | `npm test` | core package tests | not yet recorded | Use after any TS artifact or API rename. |
| `src/polyglot-notebooks-browser` | `npm run compile` | build browser bundle | not yet recorded | Produces `dist/dotnet-interactive.js` today. |
| `src/polyglot-notebooks-browser` | `npm test` | browser package tests | not yet recorded | High-value proof for browser global renames. |
| `src/polyglot-notebooks-vscode` | `npm run compile` | stable extension build | not yet recorded | Use before local VSIX installation. |
| `src/polyglot-notebooks-vscode` | `npm test` | stable extension tests | not yet recorded | Add exact test command if the package exposes a more specific target later. |
| `src/polyglot-notebooks-vscode-insiders` | `npm run compile` | insiders extension build | not yet recorded | Keep stable and insiders behavior aligned. |
| `src/polyglot-notebooks-vscode-insiders` | `npm test` | insiders extension tests | not yet recorded | Add exact test command if the package exposes a more specific target later. |

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
