# AGENTS.md

This repository contains two Unity packages inside one Unity project:
- `Assets/vFrame.Core`: engine-agnostic C# runtime library.
- `Assets/vFrame.Core.Unity`: Unity-specific runtime library depending on `vFrame.Core`.

## Scope
- Scope: the entire repository rooted at `D:\Workspace\vFrame\vFrame.Core`.
- No Cursor rules were found in `.cursor/rules/` or `.cursorrules`.
- No Copilot instructions were found in `.github/copilot-instructions.md`.

## Repository Facts
- Unity version: `2021.3.32f1` from `ProjectSettings/ProjectVersion.txt`.
- Package compatibility in both `package.json` files: `2018.4`.
- Runtime assemblies:
  - `Assets/vFrame.Core/Runtime/vFrame.Core.asmdef`
  - `Assets/vFrame.Core.Unity/Runtime/vFrame.Core.Unity.asmdef`
- No test assemblies or test source files are currently checked in.
- CI only publishes UPM branches; it does not run build, lint, or test jobs.

## Directory Guide
- `Assets/vFrame.Core/Runtime`: core library code with no `UnityEngine` dependency.
- `Assets/vFrame.Core.Unity/Runtime`: Unity-facing runtime code.
- `.editorconfig`: authoritative formatting and naming rules.
- `.github/workflows/*.yml`: UPM publishing automation.

## Workflow
- Make focused, minimal changes.
- Preserve public APIs unless the task explicitly requires API changes.
- Treat this as a library repo: avoid scene edits, editor-only assumptions, or app-specific behavior unless requested.
- Keep `vFrame.Core` free of `UnityEngine` references.

## Build Commands
Use Unity batch mode for authoritative compilation. There is no dedicated non-Unity build script.
Set an editor path first if needed:

```powershell
$UNITY="C:\Program Files\Unity\Hub\Editor\2021.3.32f1\Editor\Unity.exe"
```

Open/import the project once:

```powershell
& $UNITY -batchmode -quit -projectPath "D:\Workspace\vFrame\vFrame.Core" -logFile -
```

Compile scripts / validate project load:

```powershell
& $UNITY -batchmode -quit -projectPath "D:\Workspace\vFrame\vFrame.Core" -logFile -
```

Notes:
- Unity compilation errors surface in the batch-mode log.
- `vFrame.Core` and `vFrame.Core.Unity` are shipped as UPM packages via Git branches.

## Test Commands
There are no tests checked into this repository today. If you add tests, use the Unity Test Framework CLI.
All EditMode tests:

```powershell
& $UNITY -batchmode -quit -projectPath "D:\Workspace\vFrame\vFrame.Core" -runTests -testPlatform EditMode -logFile - -testResults "TestResults/editmode-results.xml"
```

All PlayMode tests:

```powershell
& $UNITY -batchmode -quit -projectPath "D:\Workspace\vFrame\vFrame.Core" -runTests -testPlatform PlayMode -logFile - -testResults "TestResults/playmode-results.xml"
```

Single test by full name:

```powershell
& $UNITY -batchmode -quit -projectPath "D:\Workspace\vFrame\vFrame.Core" -runTests -testPlatform EditMode -testFilter "Namespace.ClassName.TestMethod" -logFile - -testResults "TestResults/single-test.xml"
```

Single fixture / class:

```powershell
& $UNITY -batchmode -quit -projectPath "D:\Workspace\vFrame\vFrame.Core" -runTests -testPlatform EditMode -testFilter "Namespace.ClassName" -logFile - -testResults "TestResults/single-fixture.xml"
```

Testing guidance:
- Prefer EditMode tests for `Assets/vFrame.Core` unless Unity runtime behavior is required.
- Use PlayMode tests only for code that genuinely depends on scene objects or frame execution.
- Put tests in dedicated test assemblies, not runtime folders.

## Lint And Formatting
There is no standalone linter config such as Roslyn analyzers, StyleCop, or `dotnet format` committed in this repo.
Style sources:
- `.editorconfig`
- existing code in `Assets/vFrame.Core/Runtime`
- existing code in `Assets/vFrame.Core.Unity/Runtime`

Follow `.editorconfig`, keep formatting consistent with adjacent files, prefer IDE formatting on touched files only, and do not mass-reformat unrelated files.

## Formatting Rules
- Charset: `utf-8-bom`.
- Line endings: `crlf`.
- Indentation: 4 spaces for C#.
- Max line length: 120.
- Trim trailing whitespace: enabled.
- Final newline: disabled globally in `.editorconfig`; preserve file behavior.
- Namespace style: block-scoped namespaces, not file-scoped namespaces.
- Opening braces for types are on a new line; method braces usually stay on the declaration line.
- Braces are required for `if`, `foreach`, `for`, and `while`.

## Imports
- Keep `using` directives at the top of the file, outside the namespace.
- Order usings with framework namespaces first, project namespaces after.
- Keep alias imports only when needed to resolve ambiguity, for example `Logger = vFrame.Core.Loggers.Logger`.
- Remove unused usings when touching a file.
- Do not introduce `global using` directives.

## Naming Conventions
- Namespaces use PascalCase segments under `vFrame.Core...`.
- Public types, methods, properties, and constants use PascalCase.
- Private fields use `_camelCase`.
- Private static readonly fields use `_camelCase` per `.editorconfig`.
- Unity serialized fields should also use `_camelCase`.
- Interface names use the `I` prefix.
- Use `nameof(...)` for parameter names when throwing guard exceptions.

## Types And Language Usage
- `var` is preferred when the type is apparent, matching `.editorconfig` and existing code.
- Use explicit types when clarity is better than `var`.
- Keep generics straightforward; this codebase already uses generic base types heavily.
- Do not introduce nullable reference type annotations unless the surrounding file or package is already using them consistently.
- Avoid adding modern syntax that materially changes style across a file unless required.
- Avoid LINQ in hot paths unless readability clearly outweighs allocation/perf cost.

## Error Handling
- Prefer guard clauses near method entry.
- Reuse `vFrame.Core.Exceptions.ThrowHelper` where the repo already uses it.
- Throw precise exceptions for invalid input, unsupported enum values, invalid state, and type mismatches.
- Use `nameof(param)` instead of string literals for parameter names.
- Catch exceptions only when isolation and logging are part of the existing design.
- When catching exceptions, log enough context to diagnose the failing operation.
- Do not swallow exceptions silently.

## State And Lifetime Patterns
- Respect the `BaseObject` lifecycle: `Create(...)`, `Destroy()`, `OnCreate(...)`, `OnDestroy()`.
- Call `ThrowIfDestroyed()`, `ThrowIfNotCreated()`, or `ThrowIfNotCreatedOrDestroyed()` where state invariants matter.
- Preserve the repo's pattern of using `try/finally` to update lifecycle flags.
- For pooled or reusable objects, clear owned state on destroy/recycle.

## Collections And Allocation
- This library is allocation-conscious.
- Reuse the existing object pool abstractions instead of allocating temporary objects in hot code when a pool already exists.
- In mutation-sensitive loops, prefer index-based loops when collection contents may change during dispatch.
- Be careful with allocations from LINQ, closures, and temporary lists.

## Logging
- Use the existing logging abstractions in `vFrame.Core.Loggers` instead of ad hoc logging APIs.
- In Unity-specific code, `UnityLogger` bridges library logs into `UnityEngine.Debug`.
- Prefer structured context in log messages.

## Unity-Specific Guidance
- Keep `Assets/vFrame.Core` free from `UnityEngine` references.
- Put Unity-only behavior in `Assets/vFrame.Core.Unity`.
- Use existing Unity lifetime APIs and extensions before adding wrappers.
- Avoid editor-only APIs in runtime assemblies.

## Change Hygiene
- Do not add new dependencies unless the task explicitly requires them.
- Do not rename files, namespaces, or public symbols without a concrete reason.
- Keep comments sparse; prefer self-explanatory code.
- Match the existing file header/comment style only when editing a file that already uses it.
- If you add tests, include the exact Unity CLI command used to run them in your handoff.

## Validation Expectations For Agents
- For small changes, at minimum run Unity batch mode once to catch compilation errors.
- For test changes, run the narrowest relevant Unity test filter first, then broaden if needed.
- Report missing local prerequisites such as an unavailable Unity editor path.
