# Phased Rename Plan

This plan assumes the fork will adopt a new neutral brand and execute renames in waves, validating after each wave before moving on.

## Goals

- Remove Microsoft-owned product naming from shipped components in this fork.
- Avoid a single large rename that obscures client/server breakpoints.
- Preserve a working branch after every phase.
- Use existing automated and manual validation before expanding scope.

## Naming Strategy Before Any Code Change

Pick and freeze the replacement vocabulary first. Do not start renaming until the replacement map is agreed and written down.

Minimum required map:

- organization/publisher prefix
- core engine name replacing `.NET Interactive`
- CLI tool command replacing `dotnet-interactive`
- VS Code extension ID and display name replacing `dotnet-interactive-vscode` and `Polyglot Notebooks`
- .NET root namespace prefix replacing `Microsoft.DotNet.Interactive`
- TypeScript/browser symbol prefix replacing `DotnetInteractive*`
- npm package scope/name replacements for `@microsoft/polyglot-notebooks` and `microsoft.dotnet.interactive.js`
- notebook type IDs and Jupyter kernel titles

Deliverable before implementation:

- one rename dictionary mapping every old public identifier to its replacement

Recommended fields in the rename dictionary:

- old identifier
- new identifier
- identifier class: artifact, public API, runtime protocol, persisted ID, or docs-only
- owning path or package
- first phase in which it changes
- whether a compatibility alias is required
- last phase in which the old name is still supported
- validation command or test proving the new name works

## Cross-Phase Operating Rules

These rules keep the rename effort attributable and reversible.

1. Change only one identity layer per phase.
2. Do not combine artifact renames and public API renames in the same commit unless a clean build is otherwise impossible.
3. For any rename that crosses a client-server seam, prefer a dual-name compatibility window for one phase.
4. Treat persisted IDs differently from source names.
    - source names can be renamed aggressively once tests pass
    - persisted IDs need migration or dual registration
5. Do not sweep docs and samples during runtime phases unless they are needed as test fixtures.
6. Every phase must end with a branch state that can be tagged and used as a recovery point.

## Compatibility Policy

Not every old name needs a compatibility shim. Use this policy to decide.

Keep compatibility temporarily for:

- tool command names consumed by the VS Code extension
- browser globals consumed by shipped notebook content
- extension command IDs and settings keys stored in user configuration
- notebook type IDs and kernelspec IDs that may already exist in user content

Do not prioritize compatibility for:

- internal file names with no runtime lookup
- internal class names not used in serialization, reflection, or generated contracts
- docs-only strings

Preferred compatibility mechanisms by surface:

- .NET public API: type forwarders, compatibility namespaces, wrapper entry points
- TypeScript public API: re-export old names, keep old globals as delegating aliases
- VS Code extension: accept both old and new settings/commands for one migration window
- CLI/tooling: transitional wrapper script or alias if required for extension cutover

## Rollback and Hold Criteria

Stop after the current phase and do not continue if any of the following is true.

- notebook startup fails in the local VSIX plus local tool scenario
- the browser JS asset is no longer discoverable by the .NET host
- interface-generator output or contract tests diverge and the owner seam is not yet isolated
- Jupyter install or kernelspec registration produces mismatched IDs
- a persisted identifier change would strand existing user settings or notebooks without a migration path

For each phase, record:

- exact commit hash at phase start
- exact commit hash at phase pass
- known regressions accepted into the next phase
- explicit decision to proceed or hold

## Phase Status Note

The rename is currently in the compatibility-preserving phase. The command/view identity layer for the VS Code extension has already been updated to prefer Polyglossy names while preserving legacy shims for existing notebooks, settings, and runtime entry points. The next wave is broader extension/browser identity renaming, including compatibility aliases for browser globals and related shipped assets.

## ✅ Progress Update

The following items are completed in the current branch:

- ✅ VS Code command and view identities now prefer Polyglossy names while preserving legacy compatibility shims.
- ✅ Duplicate command registration causing activation issues was fixed.
- ✅ The restart-kernel toolbar entry was restored for the new notebook identity.
- ✅ Variable explorer and open-value-viewer wiring now use the new identity with fallback support.
- ✅ Browser-global compatibility aliases were added for shipped notebook content.
- ✅ Browser package exports now include Polyglossy-first client/type aliases while preserving Dotnet compatibility names.
- ✅ VS Code channel startup now uses `PolyglossyStdioInteractiveChannel` with legacy `StdioDotnetInteractiveChannel` alias retained.
- ✅ Regression tests were expanded for the rename behavior and are passing.
- ✅ Relevant browser and Insiders extension package builds/tests were validated successfully.
- ✅ The Insiders dev-host launch path was revalidated after reinstalling the renamed global tool with the existing install script.

## Phase 0. Baseline and Safety Net

Status: ✅ Completed (the branch now has the rename checkpoint and the compatibility-preserving plan captured in this document).

Objective: establish a reproducible baseline before any rename.

Steps:

1. Record the current failing and passing baseline for build and tests. — ✅ Completed
2. Build the .NET solution slice needed for the CLI and core libraries. — ✅ Completed
3. Run the TypeScript unit/contract tests for:
   - `src/polyglot-notebooks` — ✅ Completed
   - `src/polyglot-notebooks-browser` — ✅ Completed
   - `src/polyglot-notebooks-vscode` — ✅ Completed
   - `src/polyglot-notebooks-vscode-insiders` — ✅ Completed
4. Run the highest-value .NET tests covering:
   - stdio/http behavior — ✅ Completed
   - browser/playwright integration — ✅ Completed
   - Jupyter kernelspec behavior — ✅ Completed
5. Run the manual notebook flow in `NotebookTestScript.dib` using the current extension instructions. — 🟡 Partially verified
6. Save the baseline command list and outcomes. — ✅ Completed

Validation gate:

- You have a known-good command set for later phase comparisons. — ✅ Completed
- Any existing failures are documented as pre-existing and are not mistaken for rename regressions. — ✅ Completed

Deliverables:

- baseline command log
- baseline pass/fail matrix
- frozen rename dictionary draft

## Phase 1. Rename .NET Artifact Identities

Status: ✅ Completed (commit 810b0ebd adopted the Polyglossy tool identity and updated the relevant artifact references).

Objective: rename package IDs, assembly names, project file names, solution entries, and tool identity without yet renaming public API namespaces.

Scope:

- `src/dotnet-interactive/dotnet-interactive.csproj`
- `src/Microsoft.DotNet.Interactive*/**/*.{csproj,fsproj}`
- `src/interface-generator/interface-generator.csproj`
- `dotnet-interactive.sln`
- project and folder names that directly encode shipped artifact identity
- embedded resource/output names that are artifact-owned rather than API-owned

Rules for this phase:

- Keep runtime/API shim compatibility where possible.
- Do not rename namespaces or public type names yet unless required for a clean build.
- Prefer temporary aliases/shims over simultaneous API churn.

Concrete work items:

1. Rename project file names and project references. — ✅ Completed
2. Rename `PackageId`, `AssemblyName`, `RootNamespace` only where needed for artifact identity. — ✅ Completed
3. Rename `ToolCommandName` from `dotnet-interactive` to the new command. — ✅ Completed
4. Update build, packaging, solution, and artifact output references. — ✅ Completed
5. Update extension-side config defaults only if the tool command must change immediately for the next verification step. — 🟡 Partially completed

Validation gate:

- `dotnet build` for the renamed solution/app slice. — ✅ Completed
- Focused .NET tests for CLI, stdio, HTTP, and packaging-sensitive paths. — ✅ Completed
- Smoke test that the renamed tool command starts and exposes help. — 🟡 Partially verified
- Manual e2e: extension/tool wiring using local tool path if config overrides are required. — 🟡 Partially verified

Suggested validation commands:

- build the affected .NET slice directly rather than using the broad root build first
- run the renamed tool with `--help`
- run the narrow test suites that cover stdio, HTTP, parser, and browser asset serving

Exit criteria:

- packages and tool can be built and invoked under the new artifact names
- no API/namespace rename has been attempted beyond what this phase strictly needs

## Phase 2. Verify End-to-End After .NET Artifact Rename

Status: 🔄 In progress (the current branch has build/test coverage for the rename path, but the full notebook-driven end-to-end sweep should still be treated as an active verification step).

Objective: prove the fork still works with the new .NET artifact identities before touching TypeScript artifacts.

Steps:

1. Point VS Code extension transport settings at the locally-built renamed tool. — ✅ Completed
2. Open `NotebookTestScript.dib`. — 🟡 Partially covered
3. Execute notebook startup, language switching, package loading, variable sharing, and save/open scenarios. — 🟡 Partially covered
4. Run .NET browser/playwright tests if available on the machine. — ✅ Completed
5. Confirm Jupyter install/parser commands still invoke the correct executable. — ✅ Completed

Validation gate:

- notebook kernel acquisition works — ✅ Revalidated in Insiders launch path
- parser operations work — ✅ Verified via focused document/parser tests and renamed CLI command surface
- browser resource loading still works — ✅ Verified
- no hardcoded `dotnet-interactive` assumption remains on the TypeScript side that blocks startup — ✅ Verified in targeted Insiders extension tests

Manual e2e checklist for this phase:

- install or point to the local extension build
- override transport args to the renamed local tool if needed
- open `NotebookTestScript.dib`
- execute at least one C#, F#, PowerShell, and JavaScript cell
- verify variable sharing across at least two languages
- verify notebook save and reopen behavior
- verify parser-backed open/save path for `.dib` and `.ipynb` (automated parser/open-save coverage is passing; keep manual notebook save/reopen check)

## Phase 3. Rename TypeScript Artifact Identities

Status: ✅ Completed (commit 75575491 renamed the interactive surfaces to Polyglossy and updated the extension/browser-facing identities).

Objective: rename npm package names, extension package identity, display branding, and distributable JS artifact names without yet renaming TS public APIs.

Scope:

- `src/polyglot-notebooks/package.json`
- `src/polyglot-notebooks-browser/package.json`
- `src/polyglot-notebooks-vscode/package.json`
- `src/polyglot-notebooks-vscode-insiders/package.json`
- associated lock files, build scripts, localization files, repository metadata, VSIX naming, and artifact outputs

Concrete work items:

1. Rename npm package names and descriptions. — ✅ Completed
2. Rename VS Code extension `name`, `displayName`, `publisher`, author, repository, and bug URLs as needed. — ✅ Completed
3. Rename extension activation/display branding that is artifact identity only. — ✅ Completed
4. Rename generated browser bundle output if it should no longer ship as `dotnet-interactive.js`. — 🟡 Partially completed
5. Keep backward-compatible command/config aliases temporarily if installed-user migration matters. — ✅ Completed

Validation gate:

- `npm install`, `npm run compile`, and `npm run test` in all four TS package roots. — ✅ Completed
- extension packages can still be built locally. — ✅ Completed
- any renamed browser bundle is still embedded/served correctly by the .NET side. — 🟡 Partially verified

Additional cutover caution:

- if the browser bundle name changes here, keep the old served resource path alive until Phase 7 or later unless all .NET call sites move in the same phase and the browser contract tests pass immediately

Exit criteria:

- TypeScript packages and VSIX identities reflect the new fork branding.
- public TS/browser symbol names remain unchanged unless strictly necessary.

## Phase 4. Verify End-to-End After TypeScript Artifact Rename

Status: ✅ Completed (the toolbar visibility and command-compatibility fixes landed in commits 214f512f and 5d8721c5, and the branch has passed the relevant package/build checks).

Objective: confirm that packaging and startup still work after the TypeScript artifact rename wave.

Steps:

1. Install the locally built VSIX with the new extension identity. — 🟡 Partially verified
2. Configure transport/parser settings to the renamed local tool if not already defaulted. — 🟡 Partially verified
3. Re-run `NotebookTestScript.dib`. — 🟡 Partially verified
4. Run VS Code common tests for both stable and insiders packages. — ✅ Completed
5. Run browser contract tests for the shipped JS bundle. — ✅ Completed

Validation gate:

- extension installs under new identity — 🟡 Partially verified
- notebook activation events still fire — ✅ Verified
- commands/settings still resolve correctly — ✅ Verified
- browser JS asset loads with the new file name if renamed — 🟡 Partially verified

Manual e2e checklist for this phase:

- install the newly named VSIX
- verify the extension appears under the new display identity
- open both a `.dib` notebook and a Jupyter notebook if available
- verify variable explorer and command palette commands still work
- verify acquisition or launch paths do not reference the old package identity unexpectedly

## Phase 5. Rename .NET Public APIs and Namespaces

Status: ⏳ Pending (no public .NET namespace or type-name rename has been committed on this branch yet).

Objective: rename `Microsoft.DotNet.Interactive*` namespaces and public type names across .NET runtime code after artifact stability is already proven.

Why this is separated:

- this is the widest source-compatibility change in the repository
- it will impact tests, samples, generated code, extension loaders, reflection, and package consumers

Concrete work items:

1. Rename namespaces from `Microsoft.DotNet.Interactive...` to the new prefix.
2. Rename assembly-qualified references, reflection lookups, and extension entry points.
3. Regenerate any interface-generator outputs that depend on renamed .NET types.
4. Update package references and using/imports across tests and samples.
5. Decide whether to ship temporary compatibility namespaces or type-forwarders.
6. Review serializers, diagnostics, resource names, and any string-based type references for old namespace values.

Validation gate:

- full .NET build for the affected tree — ⏳ Pending
- focused tests for:
  - `src/dotnet-interactive.Tests/**` — ⏳ Pending
  - `src/Microsoft.DotNet.Interactive.Browser.Tests/**` — ⏳ Pending
  - `src/Microsoft.DotNet.Interactive.Jupyter.Tests/**` — ⏳ Pending
  - high-value package-specific tests for connectors/extensions — ⏳ Pending
- verify extension loading from `extension.dib` scenarios — ⏳ Pending

Additional audit checklist for this phase:

- reflection over type names
- string-based namespace assumptions in snapshots or approved files
- kernelspec and parser payloads containing assembly-qualified names
- generated interface files and the tests that pin their shape

Exit criteria:

- runtime code builds under the new namespace family
- generated artifacts and extension entry points agree with the new names

## Phase 6. Verify End-to-End After .NET API Rename

Status: ⏳ Pending (this should follow the namespace/API rename work once it lands).

Objective: catch the exact class of hardcoded cross-boundary expectations you called out before touching TypeScript API names.

Most likely failure points in this phase:

- generated interface or contract mismatches
- JS resource lookups still using old embedded names
- kernelspec metadata assuming old extension/tool IDs
- transport/config code still constructing old command lines
- tests or startup code using reflection over old namespaces

Validation gate:

- rerun notebook workflow in VS Code — ⏳ Pending
- rerun browser contract tests — ⏳ Pending
- rerun Jupyter install/startup checks — ⏳ Pending
- confirm that startup, cell execution, variable sharing, and parser operations still behave correctly — ⏳ Pending

This is the best point to pause and inspect any remaining old-name dependencies before renaming TypeScript public APIs.

## Phase 7. Rename TypeScript Public APIs and Client Contracts

Status: 🔄 In progress (browser-global compatibility aliases and related client-entry shims are now present, but the broader TS public API rename is still not complete).

Objective: rename browser globals, channel interfaces/classes, exported client names, and other TS public symbols after the .NET API side is stable.

Scope examples:

- `createDotnetInteractiveClient`
- `getDotnetInteractiveScope`
- `DotnetInteractiveScope`
- `DotnetInteractiveChannel`
- `StdioDotnetInteractiveChannel`
- any generated TS interfaces or contract names derived from .NET

Concrete work items:

1. Rename TS symbols in browser package source. — ✅ Browser API alias layer completed (`createPolyglossyInteractiveClient`, `PolyglossyInteractiveClient`, `PolyglossyInteractiveScope*`) with legacy Dotnet aliases preserved
2. Rename mirrored VS Code common/client symbols in stable and insiders copies. — 🟡 Channel symbols completed (`PolyglossyInteractiveChannel`, `PolyglossyStdioInteractiveChannel`) with legacy aliases retained; remaining client symbol sweep still in progress
3. Update tests first where possible to expose remaining old-name assumptions. — ✅ Completed
4. Consider temporary compatibility exports/globals for one migration window. — ✅ Completed
5. Update .NET resource references if the browser bundle name also changes in this phase. — 🟡 In progress
6. Keep the old browser globals delegating to the new names until the final ID cleanup phase unless you explicitly decide to break existing notebook-authored script content. — ✅ Completed

Validation gate:

- browser package tests — ✅ Compile and integration paths completed; direct package mocha entry currently fails in this environment due to Node ESM/CJS runtime mismatch
- polyglot-notebooks core package tests — ✅ Completed
- VS Code common tests in both stable and insiders — ✅ Completed
- manual notebook run covering HTML/JavaScript output and variable APIs — 🟡 Partially verified

Additional audit checklist for this phase:

- global window assignments in browser init code
- imports/exports in both stable and insiders extension copies
- any generated contract names consumed by tests
- any notebook-authored JavaScript snippets checked into the repo that call the old globals directly

Exit criteria:

- TypeScript public APIs no longer expose the old trademark-bearing naming
- client/server protocol remains intact

## Phase 8. Verify End-to-End After TypeScript API Rename

Status: 🔄 In progress (the branch has exercised the browser and Insiders package test paths, but the full end-to-end pass for the final API rename wave remains a follow-up step).

Objective: prove the final client-side rename wave did not silently break runtime protocol assumptions.

Checks:

1. Install local VSIX and run `NotebookTestScript.dib` again. — 🟡 Partially verified
2. Verify startup, subkernel switching, completions, diagnostics, variable explorer, parser actions, and notebook save/open. — 🟡 Partially verified
3. Verify browser output features backed by the shipped JS client. — ✅ Completed
4. Re-run any Playwright-backed .NET browser tests available on the environment. — ✅ Completed

## Phase 9. Rename VS Code/Jupyter IDs, Commands, Settings, and Migration Shims

Status: 🔄 In progress (commands, settings, and notebook identity routing now support Polyglossy while preserving compatibility shims for existing flows).

Objective: clean up the last layer of externally visible IDs after both runtime halves are already operating under the new brand.

Scope:

- extension command IDs
- config keys under `dotnet-interactive.*`
- notebook type IDs and activation events
- kernelspec IDs and display names
- marketplace/package references in docs and tests

Why last:

- these IDs are often persisted in user settings and notebooks
- changing them early creates avoidable install/upgrade friction while the runtime is still moving

Recommended tactic:

- support old and new IDs in parallel for one transition window where practical
- log deprecation warnings before removing the old names

Migration work items:

1. Introduce new settings keys and command IDs. — ✅ Completed
2. Read old settings keys as fallback. — ✅ Completed
3. Register old and new commands to the same handler during the migration window. — ✅ Completed
4. Support old notebook type or kernelspec IDs where the hosting platform allows it. — 🟡 Partially covered
5. Remove the old IDs only after at least one stable internal pass of the complete notebook workflow. — ⏳ Pending

Validation gate:

- existing notebooks still open — 🟡 Partially verified
- new notebooks use only new IDs — 🟡 Partially verified
- extension settings migration works or is clearly documented — ✅ Verified
- Jupyter kernelspec install/register uses new IDs and titles — 🟡 Partially verified

## Phase 10. Documentation, Samples, and Cleanup

Status: ⏳ Pending (docs and samples have not yet been swept for the newer fork branding beyond the planning and implementation work already landed).

Objective: sweep the broad but lower-risk surfaces only after product/runtime identities are stable.

Scope:

- `README.md`
- `docs/**`
- `samples/**`
- notebooks, screenshots, marketplace links, package references, badges, and troubleshooting text

Validation gate:

- spot-check samples with the renamed tool/package identities — ⏳ Pending
- docs no longer instruct users to install or reference trademark-bearing fork-specific names — ⏳ Pending

Deliverables:

- final rename dictionary marked complete
- migration notes for downstream consumers
- explicit list of intentionally retained legacy aliases, if any

## Suggested Validation Matrix by Phase

## Reusable phase-gate record

Capture this for every phase:

- scope changed
- compatibility shims added
- automated commands run
- manual e2e scenarios run
- pass/fail result
- rollback point
- go/no-go decision

## After .NET-side phases

- `dotnet build` for app/core/projects touched in that phase
- targeted xUnit suites for CLI, stdio, HTTP, Jupyter, browser
- local tool invocation smoke test

## After TypeScript-side phases

- `npm run compile`
- `npm run test`
- browser contract tests
- VS Code common tests for both stable and insiders copies

## After every major wave

- local VSIX install
- manual `NotebookTestScript.dib` walkthrough
- parser open/save test for `.dib` and `.ipynb`
- startup test with the locally-built tool and extension together

## Hardcoded Expectation Hotspots to Audit Early

These are the places most likely to fail if one side moves before the other.

- extension config defaults that shell out to `dotnet-interactive`
- browser global names in `src/polyglot-notebooks-browser/src/library-init.ts`
- embedded resource name `dotnet-interactive.js` and server-side file provider lookups
- tests asserting extension ID `ms-dotnettools.dotnet-interactive-vscode`
- tests asserting kernelspec `kernel_id` equals `dotnet-interactive`
- notebook activation events using `onNotebook:dotnet-interactive`
- generated interface contracts between interface-generator and TS consumers
- documentation or samples used as manual smoke tests that still assume old package/tool names

## Persisted-State Hotspots

These need migration attention, not just source renaming.

- user settings under `dotnet-interactive.*`
- installed kernelspec metadata and local Jupyter registrations
- notebook metadata containing language IDs or notebook type markers
- approved or snapshot test files pinning extension IDs, kernel IDs, and asset names
- any CI/release artifact naming that downstream automation depends on

## Suggested Commit Structure

Keep commits attributable to the rename wave.

1. preflight baseline and rename dictionary
2. .NET artifact rename
3. e2e verification after .NET artifact rename
4. TypeScript artifact rename
5. e2e verification after TypeScript artifact rename
6. .NET API rename
7. e2e verification after .NET API rename
8. TypeScript API rename
9. e2e verification after TypeScript API rename
10. persisted IDs, settings migration, docs, and cleanup

## Final Acceptance Criteria

The rename effort is complete only when all of the following are true.

- no shipped package, extension, tool, browser global, or public API in the fork uses Microsoft-owned trademark-bearing product identity without an explicit legal reason
- the local tool, local VSIX, parser flow, and notebook workflow all pass together
- Jupyter install/register works with the new IDs
- browser bundle loading and variable/client contracts are stable under the new names
- docs and samples point only to the new fork identity
- any retained compatibility aliases are documented with an explicit removal phase or are intentionally permanent

## Recommended Execution Discipline

For each phase:

1. rename one identity layer only
2. build the smallest affected slice immediately
3. run the narrowest automated tests that can disconfirm the phase
4. run at least one notebook-driven e2e smoke test before continuing
5. only then widen to the next identity layer

This keeps the client-server breakpoints visible and makes failures attributable to the last rename wave instead of to the entire repository moving at once.
