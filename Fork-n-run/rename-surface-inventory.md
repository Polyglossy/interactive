# Rename Surface Inventory

This inventory groups the concrete rename surfaces that will need attention when removing Microsoft-owned trademarks from the fork.

## 1. .NET Artifact Identity Surfaces

These are the highest-risk packaging and assembly identifiers because they flow into NuGet, the .NET tool, assembly names, root namespaces, embedded resources, and test expectations.

- `src/dotnet-interactive/dotnet-interactive.csproj`
  - `PackageId`: `Microsoft.dotnet-interactive`
  - `ToolCommandName`: `dotnet-interactive`
  - `AssemblyName`: `Microsoft.DotNet.Interactive.App`
  - `RootNamespace`: `Microsoft.DotNet.Interactive.App`
  - embedded JS resource path includes `dist/dotnet-interactive.js`
- `src/Microsoft.DotNet.Interactive/Microsoft.DotNet.Interactive.csproj`
  - `PackageId`: `Microsoft.DotNet.Interactive`
- `src/Microsoft.DotNet.Interactive.*/*.{csproj,fsproj}`
  - package IDs, assembly names, project references, and root namespaces consistently use `Microsoft.DotNet.Interactive.*`
- `src/interface-generator/interface-generator.csproj`
  - `AssemblyName` and `RootNamespace` use `Microsoft.DotNet.Interactive.InterfaceGen.App`
- `dotnet-interactive.sln`
  - project names and paths expose current product naming throughout the solution graph

## 2. .NET Public API and Namespace Surfaces

These affect consumers, source compatibility, reflection, serialization, generated code, and tests.

- `namespace Microsoft.DotNet.Interactive...` throughout `src/Microsoft.DotNet.Interactive*`
- `using Microsoft.DotNet.Interactive...` throughout runtime code, tests, samples, and docs
- extension entry points such as `Microsoft.DotNet.Interactive.DuckDB.KernelExtension.Load(...)`
- CLI/application namespaces under `Microsoft.DotNet.Interactive.App...`
- generated or contract-sensitive names referenced by tests such as `TypeScriptInterfacesContractTests`

## 3. TypeScript Package and Browser API Surfaces

These are the main TypeScript/public JavaScript branding surfaces.

- `src/polyglot-notebooks/package.json`
  - package name: `@microsoft/polyglot-notebooks`
- `src/polyglot-notebooks-browser/package.json`
  - package name: `microsoft.dotnet.interactive.js`
  - description and author reference `Microsoft`
  - output artifact name is `dist/dotnet-interactive.js`
- `src/polyglot-notebooks-browser/src/library-init.ts`
  - global functions: `getDotnetInteractiveScope`, `createDotnetInteractiveClient`
  - runtime type names: `DotnetInteractiveScopeContainer`, `DotnetInteractiveScope`
- `src/polyglot-notebooks-vscode-common/src/DotnetInteractiveChannel.ts`
  - interface name `DotnetInteractiveChannel`
- `src/polyglot-notebooks-vscode-common/src/stdioDotnetInteractiveChannel.ts`
  - class name `StdioDotnetInteractiveChannel`
- mirrored copies of the same channel/client names exist in:
  - `src/polyglot-notebooks-vscode/**`
  - `src/polyglot-notebooks-vscode-insiders/**`

## 4. VS Code Extension Identity Surfaces

These are release-critical because they affect upgrade behavior, command names, settings, notebook type IDs, activation events, VSIX identity, and marketplace continuity.

- `src/polyglot-notebooks-vscode/package.json`
  - extension name: `dotnet-interactive-vscode`
  - display name: `Polyglot Notebooks`
  - publisher: `ms-dotnettools`
  - author: `Microsoft Corporation`
  - activation events include `onNotebook:dotnet-interactive`
  - command IDs include `dotnet-interactive.acquire`
  - configuration keys include:
    - `dotnet-interactive.kernelTransportArgs`
    - `dotnet-interactive.notebookParserArgs`
    - `dotnet-interactive.kernelTransportWorkingDirectory`
    - `dotnet-interactive.interactiveToolSource`
    - `dotnet-interactive.minimumDotNetSdkVersion`
    - `dotnet-interactive.requiredInteractiveToolVersion`
  - Jupyter kernel titles expose `.NET Interactive`
- `src/polyglot-notebooks-vscode-insiders/package.json`
  - same rename class as stable package
- localization files under both extension folders
  - `package.nls*.json` descriptions and titles must stay aligned with renamed IDs

## 5. Notebook, Jupyter, and Parser Identity Surfaces

These are cross-boundary IDs that can silently break if the client and server stop agreeing.

- notebook activation and notebook type IDs:
  - `dotnet-interactive`
  - `dotnet-interactive-window`
  - `polyglot-notebook`
  - `polyglot-notebook-window`
- Jupyter kernel display titles in extension manifests:
  - `.NET Interactive (C#)`
  - `.NET Interactive (F#)`
  - `.NET Interactive (PowerShell)`
- kernelspec metadata in tests references:
  - extension ID `ms-dotnettools.dotnet-interactive-vscode`
  - kernel ID `dotnet-interactive`
- parser command wiring in extension config currently shells out to:
  - `dotnet tool run dotnet-interactive -- notebook-parser`

## 6. Cross-Boundary Resource and Protocol Surfaces

These are the places most likely to break when one side is renamed before the other.

- JS resource name `dotnet-interactive.js`
  - embedded by `src/dotnet-interactive/dotnet-interactive.csproj`
  - served and requested by .NET tests and browser/client code
- browser global contract names:
  - `createDotnetInteractiveClient`
  - `getDotnetInteractiveScope`
- VS Code transport/config keys that inject tool command arguments
- interface-generator output and contract tests connecting .NET-generated types to TS expectations

## 7. Automated Validation Surfaces Already Present

These existing tests are useful as safety rails during phased renames.

- .NET browser/playwright-based tests:
  - `src/Microsoft.DotNet.Interactive.Browser.Tests/*.cs`
- .NET HTTP/stdio/Jupyter/CLI tests:
  - `src/dotnet-interactive.Tests/**`
  - `src/Microsoft.DotNet.Interactive.Jupyter.Tests/**`
- TypeScript browser contract tests:
  - `src/polyglot-notebooks-browser/tests/*.test.ts`
- TypeScript core package tests:
  - `src/polyglot-notebooks/tests/*.test.ts`
- VS Code common/client tests for both stable and insiders:
  - `src/polyglot-notebooks-vscode/tests/vscode-common-tests/*.test.ts`
  - `src/polyglot-notebooks-vscode-insiders/tests/vscode-common-tests/*.test.ts`
- manual notebook scenario script:
  - `NotebookTestScript.dib`
- existing guidance for extension validation:
  - `MANUAL-TESTING.md`
  - `src/polyglot-notebooks-vscode/UPDATING-TO-NEW-VERSION-OF-STABLE.md`

## 8. Documentation and Sample Surfaces

These are broad but mechanically simpler once code identities are stable.

- root docs under `docs/**`
  - product naming, tool commands, package references, marketplace URLs, screenshots, and troubleshooting text
- samples under `samples/**`
  - `using Microsoft.DotNet.Interactive...`
  - package references such as `Microsoft.DotNet.Interactive.*`
  - notebook metadata such as `languageId: dotnet-interactive.csharp`
- root README and other repository metadata files

## 9. Planned Rename Order Rationale

Recommended ordering is based on blast radius:

1. .NET artifact names first: they are the source of package/tool identity and are easier to validate with build and .NET tests.
2. TypeScript artifact names second: package/library names and output bundles can be validated with local npm tests.
3. .NET public APIs and namespaces third: this isolates source-level compatibility changes after packaging is stable.
4. TypeScript public APIs fourth: browser globals, channel names, and client contracts can then move in sync with regenerated artifacts.
5. VS Code/Jupyter identifiers and commands last: these are highest-friction for installed tooling and easiest to validate once both runtime halves are already aligned.
