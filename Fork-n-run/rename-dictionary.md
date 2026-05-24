# Rename Dictionary

Use this file to freeze the old-to-new identifier map before implementation starts. Do not begin code renames until all shipped identities that matter to a given phase have an approved replacement.

## Placeholder Tokens

Use neutral placeholders until the fork's branding is finalized.

- `<new-publisher>`: VS Code publisher or package owner.
- `<new-brand>`: product family name replacing trademark-bearing branding.
- `<new-cli-command>`: CLI/tool command replacing `dotnet-interactive`.
- `<new-dotnet-prefix>`: .NET namespace and package prefix.
- `<new-ts-prefix>`: TypeScript/browser API prefix.
- `<new-browser-bundle>`: shipped JS asset name.
- `<new-notebook-id>`: VS Code notebook type identity.
- `<new-kernel-title>`: Jupyter kernel display title.

## Status Legend

- `proposed`: candidate replacement exists but is not yet approved.
- `approved`: safe to implement in the assigned phase.
- `deferred`: intentionally kept for compatibility or later cleanup.
- `legacy-alias`: old name retained temporarily after the new name ships.

## Dictionary Fields

| Old identifier | Proposed new identifier | Identifier class | Owning path or package | First change phase | Compatibility alias | Last old-name support phase | Validation proof | Status | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `Microsoft.dotnet-interactive` | `<new-dotnet-prefix>.interactive.tool` | artifact | `src/dotnet-interactive/dotnet-interactive.csproj` | 1 | yes | 2 | renamed tool builds and `--help` works | proposed | PackageId candidate only; final casing and package convention still needed. |
| `dotnet-interactive` | `<new-cli-command>` | artifact | `src/dotnet-interactive/dotnet-interactive.csproj` | 1 | yes | 2 | local tool invocation plus VS Code transport startup | proposed | Highest-risk rename because extension and parser defaults depend on it. |
| `Microsoft.DotNet.Interactive` | `<new-dotnet-prefix>.Interactive` | public API | `src/Microsoft.DotNet.Interactive/Microsoft.DotNet.Interactive.csproj` | 5 | yes | 6 | .NET build plus focused namespace-sensitive tests | proposed | Namespace prefix should not be finalized until branding is approved. |
| `@microsoft/polyglot-notebooks` | `@<new-publisher>/<new-brand>-notebooks` | artifact | `src/polyglot-notebooks/package.json` | 3 | optional | 3 | `npm run compile` and `npm test` in `src/polyglot-notebooks` | proposed | Scope and package naming convention need approval. |
| `microsoft.dotnet.interactive.js` | `<new-browser-bundle-package>` | artifact | `src/polyglot-notebooks-browser/package.json` | 3 | optional | 3 | browser package build and tests | proposed | Keep served runtime path compatible if bundle file name moves early. |
| `dist/dotnet-interactive.js` | `dist/<new-browser-bundle>.js` | artifact | `src/polyglot-notebooks-browser/package.json` | 3 or 7 | yes | 7 | browser contract tests plus .NET asset discovery | proposed | Prefer late rename unless all .NET consumers move in the same wave. |
| `createDotnetInteractiveClient` | `create<new-ts-prefix>Client` | public API | `src/polyglot-notebooks-browser/src/library-init.ts` | 7 | yes | 8 | browser package tests and manual notebook JS output scenario | proposed | Keep old global delegating to new function for one migration window. |
| `getDotnetInteractiveScope` | `get<new-ts-prefix>Scope` | public API | `src/polyglot-notebooks-browser/src/library-init.ts` | 7 | yes | 8 | browser package tests and checked-in notebook snippets | proposed | Existing notebook-authored JavaScript may call the old global. |
| `dotnet-interactive-vscode` | `<new-brand>-vscode` | artifact | `src/polyglot-notebooks-vscode/package.json` | 3 | no | 3 | VSIX build and install succeeds | proposed | Extension marketplace identity change is one-way; treat carefully. |
| `ms-dotnettools` | `<new-publisher>` | artifact | `src/polyglot-notebooks-vscode/package.json` | 3 | no | 3 | VSIX install under the new publisher | proposed | Marketplace/publisher ownership must be settled before packaging work. |
| `dotnet-interactive.acquire` | `<new-brand>.acquire` | persisted ID | `src/polyglot-notebooks-vscode/package.json` | 8 | yes | 9 | command palette and activation tests | proposed | Old command should remain registered during the migration window. |
| `dotnet-interactive.kernelTransportArgs` | `<new-brand>.kernelTransportArgs` | persisted ID | `src/polyglot-notebooks-vscode/package.json` | 8 | yes | 9 | settings migration and notebook startup | proposed | Read old setting as fallback before removing it. |
| `dotnet-interactive.notebookParserArgs` | `<new-brand>.notebookParserArgs` | persisted ID | `src/polyglot-notebooks-vscode/package.json` | 8 | yes | 9 | parser open/save scenario | proposed | Same migration rule as transport args. |
| `polyglot-notebook` | `<new-notebook-id>` | persisted ID | `src/polyglot-notebooks-vscode/package.json` | 8 | yes | 9 | existing notebooks open, new notebooks use new type | proposed | Requires explicit migration approach because notebooks may already embed this type. |
| `.NET Interactive (C#)` | `<new-kernel-title> (C#)` | persisted ID | `src/polyglot-notebooks-vscode/package.json` | 8 | yes | 9 | Jupyter kernel registration and acquisition | proposed | Display titles can move sooner than IDs if needed. |

## Approval Checklist

Before a row changes from `proposed` to `approved`, confirm all of the following.

- The replacement does not use Microsoft-owned trademark-bearing product identity.
- The replacement matches the fork's naming convention across .NET, TypeScript, VS Code, and docs.
- The first-change phase is consistent with [phased-rename-plan.md](c:/Users/Skills/source/Polyglossy-interactive/Fork-n-run/phased-rename-plan.md).
- Compatibility intent is explicit for any client-server or persisted-state seam.
- There is at least one command or scenario that proves the new name works.

## Review Notes

- Do not guess final names in code from this file until legal/product naming is approved.
- Add rows whenever a new shipped identity is discovered in [rename-surface-inventory.md](c:/Users/Skills/source/Polyglossy-interactive/Fork-n-run/rename-surface-inventory.md).
- Keep deferred legacy aliases in this file until they are intentionally removed.
