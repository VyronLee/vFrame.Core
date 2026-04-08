<!-- GSD:project-start source:PROJECT.md -->
## Project

**vFrame.Core Improvement Program**

This project turns `vFrame.Core` into a more focused, modern, and sustainable core library for the broader `vFrame` ecosystem. It is not about expanding the repository into a full Unity framework, but about clarifying long-term boundaries, preserving the systems that matter, and planning the phased evolution of core lifecycle, messaging, pooling, logging, diagnostics, and Unity runtime integration.

The current initiative is planning-first: all 55 improvement topics have been reviewed, consensus has been captured, and the next step is to convert that consensus into a phased execution roadmap for the repository.

**Core Value:** Keep `vFrame.Core` as a lightweight, high-performance, extensible, self-directed core component library that provides stable foundational semantics for the entire `vFrame` ecosystem.

### Constraints

- **Repository Role**: `vFrame.Core` must remain a lightweight core library — it should provide primitives and conventions, not absorb unrelated framework responsibilities.
- **Architecture Boundary**: `vFrame.Core` must stay free of `UnityEngine` dependencies, while Unity-specific behavior stays in `vFrame.Core.Unity`.
- **Style Preservation**: Changes should preserve the repository's own design language and lifecycle philosophy instead of mechanically copying third-party systems.
- **Execution Order**: Testing, benchmark, CI, and debug-mode baselines should be established before major semantic refactors.
- **Migration Discipline**: Deprecated subsystems need explicit retirement or migration planning instead of being silently abandoned in place.
- **Planning Source**: The official plan should derive from the completed 55-item review record rather than from speculative re-discovery.
<!-- GSD:project-end -->

<!-- GSD:stack-start source:research/STACK.md -->
## Technology Stack

## Recommended Stack
### Core Framework
| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Unity Editor | `2021.3.32f1` baseline, plan target `Unity 6 LTS` | Authoritative compilation and package validation | Keep the current shipping line stable on the existing LTS while planning modernization against the current long-term Unity direction. Do not hard-switch the repo to Unity 6 before the validation baseline exists. |
| C# assemblies via `.asmdef` | Existing package-local assemblies | Runtime boundary enforcement | Unity requires `.asmdef` for package code, and asmdefs are the cleanest way to keep `Assets/vFrame.Core` pure C# and `Assets/vFrame.Core.Unity` Unity-only. This is the right seam for modernization work, test isolation, and package-scoped coverage. |
| UPM package layout | Standard UPM structure | Package-first library evolution | This repo ships as packages, so modernization should follow Unity’s package conventions instead of project-only layouts. That keeps tests, docs, samples, and future migration notes shippable with the package. |
### Validation Stack
| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Unity Test Framework | `1.7.x` | Functional and regression testing | This is the standard Unity-native test layer for EditMode and PlayMode. Use it because it is the authoritative, supported path for runtime/library validation from editor and CLI. |
| NUnit model inside UTF | Built into UTF custom NUnit base | Unit-style assertions and fixtures | Use plain NUnit-style EditMode tests for `vFrame.Core` logic because this package is a library, not an app. It keeps tests fast and keeps Unity runtime coupling low. |
| Unity Performance Testing package | `3.0.3` for `2021.3` compatibility | Benchmarking allocations and hot paths | vFrame.Core is allocation-conscious. Use Unity’s performance package for repeatable regression checks on pools, dispatch, and lifecycle hotspots instead of ad hoc stopwatch tests. |
| Unity Code Coverage | `1.3.x` | Coverage reports for EditMode and PlayMode | Use coverage to verify the safety baseline before large refactors. It is especially important here because the modernization plan explicitly calls for tests and diagnostics before semantic change. |
### Static Analysis And Quality Gates
| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| `.editorconfig` | Existing repo standard | Formatting and naming authority | Keep this as the primary style contract. It already matches the repository and is lower-friction than introducing a parallel style tool. |
| Unity Roslyn analyzer support | Current Unity-supported analyzer pipeline | Editor-visible diagnostics | Unity supports Roslyn analyzers and rulesets. Use this for repository-specific correctness and API-usage rules that matter to the modernization program. |
| `Microsoft.CodeAnalysis.NetAnalyzers` | Current stable package, pinned intentionally | Library-oriented code quality rules | Use this selectively for design, reliability, and performance diagnostics. Pin it deliberately rather than taking floating analyzer changes, because modernization needs stable signals, not surprise rule churn. |
| Custom vFrame analyzers | Phase-2+ optional | Enforce architectural invariants | Add custom analyzers only after the new lifecycle/message/pooling rules are stable. This is the right way to stop `UnityEngine` leakage into core and catch misuse of lifecycle contracts automatically. |
### Infrastructure
| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| GitHub Actions | Current | CI orchestration | The repo already uses GitHub Actions. Extend it rather than adding a second CI system. That keeps automation close to the package publishing workflow already in place. |
| GameCI Unity Test Runner | `v4` | Headless Unity test execution in CI | This is the standard GitHub Actions path for Unity testing. Use it to add EditMode, PlayMode, and coverage jobs without inventing custom Docker or shell glue first. |
| Unity batchmode CLI | Unity editor CLI | Local authoritative compile/test gate | Keep Unity batchmode as the source of truth locally and in fallback CI paths. It validates the actual Unity compilation environment, which matters more than standalone .NET compilation for this repo. |
| Artifact publishing in CI | `actions/upload-artifact@v4` | Preserve test, coverage, and benchmark outputs | Modernization work needs historical visibility. Save logs, coverage reports, and performance outputs as artifacts so regressions are reviewable. |
### Supporting Libraries And Tools
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| JetBrains Rider or Visual Studio | Current supported IDE | Analyzer-aware Unity C# workflow | Use one of Unity’s supported IDEs because Unity’s analyzer integration officially targets them. |
| Benchmark fixtures in `Tests/Editor` and `Tests/Runtime` asmdefs | Repo-local | Isolated validation layers | Use dedicated test assemblies per package boundary once tests are introduced. Keep most core-library tests in EditMode; reserve PlayMode for true Unity runtime behavior. |
| Samples and migration docs in UPM package structure | Repo-local | Consumer validation and upgrade guidance | Add samples only after core behavior stabilizes. They should prove the new typed messaging, lifetime, and pooling contracts to downstream users. |
## Prescriptive Stack Decision
## What Not To Use
| Category | Do Not Use | Why Not |
|----------|------------|---------|
| Dependency injection frameworks | `Zenject`, `Extenject`, `VContainer` as modernization foundation | The project context explicitly rejects external frameworks as the primary direction. They would pull the repo toward framework-level control instead of lightweight core primitives. |
| Reactive stack as core contract | `UniRx` / reactive-first architecture | Reactive APIs can be useful at the edges, but using them as the modernization backbone would over-weight a library that wants typed messages as the main interaction model. |
| Standalone `.NET` test-only validation | `dotnet test` as primary gate | This repo’s source of truth is Unity compilation and Unity package behavior. Standalone .NET checks can be supplemental later, but they cannot replace Unity validation. |
| Heavy formatting or style overlays | `StyleCop`-driven mass conformance | The repo already has `.editorconfig` and a distinct house style. Adding heavy style tooling now creates noise and slows modernization without improving architectural safety much. |
| Large observability ecosystems | `Serilog`-style external logging stack as core dependency | The program wants stronger observability, but the repo should evolve its own lightweight abstractions rather than adopt a heavy external runtime dependency tree. |
| Addressables/YooAsset in this repo’s modernization baseline | As a core stack decision for `vFrame.Core` itself | These matter for resource systems, not for the foundational modernization baseline of this library repo. They would distract from lifecycle, messages, pooling, diagnostics, and CI. |
## Alternatives Considered
| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| Validation | Unity Test Framework | Custom harnesses only | UTF is official, CLI-compatible, and package-aware. Custom harnesses increase maintenance cost and weaken standardization. |
| Performance regression checks | Unity Performance Testing | Manual stopwatch/profiler screenshots | Manual measurement is not reliable enough for a modernization program with hot-path guarantees. |
| Coverage | Unity Code Coverage | No coverage gate | Without coverage, refactors become trust-based instead of evidence-based. |
| CI | GitHub Actions + GameCI | Bespoke runners/scripts first | GameCI is the standard shortest path to Unity CI on GitHub; custom plumbing should only appear where GameCI falls short. |
| Static analysis | Roslyn analyzers + pinned NetAnalyzers | Pure manual review | Manual review alone will not consistently enforce architectural boundaries over time. |
| Unity version strategy | Stabilize on current LTS, then migrate | Immediate Unity 6 jump | A direct jump mixes platform migration risk with architecture refactor risk. Split those risks. |
## Recommended Validation Layers
### Layer 1: Compile Gate
- Unity batchmode compile on every PR.
- Fail fast on assembly/reference breakage.
### Layer 2: EditMode Tests
- Cover `BaseObject`, typed messages, pool semantics, logger abstractions, and guard/exception paths.
- This should become the main regression net because most core behavior is engine-agnostic.
### Layer 3: PlayMode Tests
- Cover only Unity-specific behavior in `vFrame.Core.Unity`, especially `SpawnPools`, lifetime integration with `GameObject`, and Unity logger bridging.
- Keep this layer smaller and slower by design.
### Layer 4: Performance Benchmarks
- Benchmark dispatch, subscription churn, pool rent/return, lifecycle creation/destruction, and common allocation paths.
- Treat benchmark regressions as review signals, not as the first hard gate.
### Layer 5: Coverage And Diagnostics
- Generate coverage reports for core assemblies.
- Publish logs, coverage, and benchmark artifacts in CI.
- Add debug/development-only instrumentation for subscriptions and pools before refactoring their internals.
## Installation
# Add/update Unity validation packages in Packages/manifest.json
# Core validation
# Analyzer direction
# Add pinned analyzer package/assets only after deciding the exact ruleset scope.
## Confidence Assessment
| Area | Confidence | Notes |
|------|------------|-------|
| Unity-native testing stack | HIGH | Verified with current Unity docs for UTF, performance testing, and coverage. |
| UPM/asmdef package-first structure | HIGH | Verified with current Unity package and asmdef docs. |
| Roslyn analyzer support in Unity | HIGH | Verified with current Unity docs. |
| GitHub Actions + GameCI recommendation | MEDIUM | Strong current ecosystem standard and current docs, but third-party rather than Unity-official. |
| Unity 6 LTS as target modernization lane | MEDIUM | Strong strategic recommendation for 2025-style planning, but this repo should not switch immediately because current validated baseline is 2021 LTS. |
## Sources
- Unity Manual: Testing your code — https://docs.unity3d.com/6000.2/Documentation/Manual/test-framework/test-framework-introduction.html
- Unity Test Framework package docs — https://docs.unity3d.com/Packages/com.unity.test-framework@1.7/manual/index.html
- Unity Performance Testing package docs — https://docs.unity3d.com/Packages/com.unity.test-framework.performance@3.3/manual/index.html
- Unity Code Coverage docs — https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.3/manual/index.html
- Unity Manual: Create or edit the assembly definitions — https://docs.unity3d.com/Manual/cus-asmdef.html
- Unity Manual: Package layout for UPM packages — https://docs.unity3d.com/Manual/cus-layout.html
- Unity Manual: Roslyn analyzers and source generators — https://docs.unity3d.com/Manual/roslyn-analyzers.html
- Microsoft Learn: Code analysis in .NET — https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview
- GameCI docs: Unity Test Runner v4 — https://game.ci/docs/github/test-runner/
<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->
## Conventions

Conventions not yet established. Will populate as patterns emerge during development.
<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->
## Architecture

Architecture not yet mapped. Follow existing patterns found in the codebase.
<!-- GSD:architecture-end -->

<!-- GSD:workflow-start source:GSD defaults -->
## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:
- `/gsd:quick` for small fixes, doc updates, and ad-hoc tasks
- `/gsd:debug` for investigation and bug fixing
- `/gsd:execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->



<!-- GSD:profile-start -->
## Developer Profile

> Profile not yet configured. Run `/gsd:profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
