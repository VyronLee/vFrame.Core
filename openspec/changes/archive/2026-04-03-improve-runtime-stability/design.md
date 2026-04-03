## Context

`vFrame.Core` and `vFrame.Core.Unity` provide shared runtime infrastructure used across lifecycle management, async execution, coroutine orchestration, pooling, downloads, and patching. The current implementation is broadly functional, but several foundational code paths have weak or incorrect state transitions, including object creation success tracking, async completion signaling, coroutine queue mutation, and threaded task failure behavior.

This change is cross-cutting because the affected code sits at the bottom of the library stack. A defect in these areas can propagate to many higher-level systems, and the current repository has no checked-in tests to lock expected behavior. The design therefore focuses on tightening runtime contracts for a narrow set of clearly incorrect behaviors while preserving existing public API shape wherever practical.

## Goals / Non-Goals

**Goals:**
- Make lifecycle and async state transitions internally consistent and predictable for `BaseObject`, `AsyncRequestCtrl`, `ThreadedTask`, and `CoroutinePool`.
- Correct known behavior mismatches in foundational runtime systems without introducing broad API churn.
- Establish a small but meaningful automated test baseline around the highest-risk runtime contracts in scope.
- Improve confidence in shared infrastructure so later refactors can build on a safer base.

**Non-Goals:**
- Redesign the entire async model across the repository.
- Replace reflection-based container messaging with a new architecture in this change.
- Refactor `ParallelTaskRunner`, `ObjectPool`, `EventDispatcher`, or patch hash validation as part of this change.
- Introduce new external dependencies or major package-level compatibility changes.

## Decisions

### Fix correctness before broad refactoring
The first priority is to correct behavior that is clearly wrong rather than redesigning multiple subsystems at once. This keeps scope small and reduces the chance that a reliability improvement unintentionally becomes an architectural rewrite.

Alternative considered:
- Perform a larger unification of async/task abstractions now.
Why not chosen:
- It would increase scope and make it harder to isolate regressions in a repository with no existing test baseline.

### Preserve public API shape where possible, tighten runtime semantics underneath
The change should prefer compatible behavior fixes over signature changes. Existing APIs such as `BaseObject.Create`, `AsyncRequestCtrl`, and `ThreadedTask` remain recognizable, but their state guarantees become stricter and more internally coherent.

Alternative considered:
- Add new replacement APIs and deprecate current ones immediately.
Why not chosen:
- That would expand the change into a migration project rather than a focused reliability improvement.

### Define reliability through observable behavioral requirements
The change will codify behavior in OpenSpec around creation success, request completion/error dispatch, task termination on failure, and safe coroutine queue mutation. This lets the implementation and tests align to a stable contract rather than ad hoc fixes.

Alternative considered:
- Treat these as implementation details only and skip spec-level requirements.
Why not chosen:
- The problems are behavioral, externally observable, and foundational enough to deserve explicit requirements.

### Add focused tests only for the highest-risk infrastructure contracts
The initial test scope should cover the failure-prone paths most likely to regress: lifecycle creation failure, async finish/error routing, threaded task fault completion, and queue-safe coroutine cancellation.

Alternative considered:
- Delay tests until after code fixes land.
Why not chosen:
- The repository currently lacks guardrails, so reliability work without tests would leave the same class of regressions likely to return.

## Risks / Trade-offs

- [Behavioral tightening may expose latent caller assumptions] -> Keep public APIs stable, document changed semantics, and target only clearly invalid prior behavior.
- [Cross-cutting fixes can create regressions in dependent systems] -> Add focused tests around each corrected runtime contract before or alongside implementation.
- [Some modules have no existing test harness] -> Start with the smallest viable Unity test assemblies and prioritize deterministic EditMode coverage.
- [Scope could expand into a full architecture cleanup] -> Keep this change limited to the four explicitly targeted subsystems and defer broader runtime improvements to follow-up changes.

## Migration Plan

This change is intended to be source-compatible for most consumers. Migration consists primarily of adopting stricter runtime expectations:

1. Correct internal implementations to match the new reliability contract.
2. Add tests that prove the corrected behavior.
3. Validate the Unity project compiles cleanly after the changes.
4. Update documentation where current wording implies behavior that is no longer accurate or was previously incorrect.

Rollback strategy:
- If a reliability fix unexpectedly breaks a consumer workflow, revert the specific behavioral correction rather than the full change set, then narrow the requirement or add a compatibility note before retrying.

## Open Questions

- Should `ThreadedTask` failure be represented only as a terminal non-blocking state for this change, with richer fault metadata deferred to a later async-contract cleanup?
