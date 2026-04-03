## Why

The repository already provides a broad set of runtime infrastructure for lifecycle management, async requests, pooling, event dispatch, downloads, and patching, but several core paths have correctness and maintainability risks. A small number of state-handling bugs and unclear runtime contracts reduce trust in the library and make future improvements riskier than they need to be.

## What Changes

- Fix core runtime state and lifecycle inconsistencies in foundational infrastructure such as object creation, async request completion, coroutine stopping, and threaded task failure handling.
- Define a narrow runtime reliability contract around these corrected behaviors so implementation and tests align to the same expectations.
- Add focused test coverage for the highest-risk lifecycle and state-machine behaviors so regressions are caught earlier.
- Clarify runtime behavior for these corrected paths without expanding this change into broader pooling, dispatcher, patching, or async architecture refactors.

## Capabilities

### New Capabilities
- `runtime-reliability`: Defines behavioral guarantees for lifecycle, async completion, task failure, pooling, and dispatcher correctness in the runtime infrastructure.

### Modified Capabilities

## Impact

- Affected code is primarily under `Assets/vFrame.Core/Runtime` and `Assets/vFrame.Core.Unity/Runtime`.
- The most impacted subsystems are `BaseObject`, `AsyncRequestCtrl`, `ThreadedTask`, and `CoroutinePool`.
- Public APIs should remain largely stable, but runtime behavior will become stricter and more predictable in edge cases and failure paths.
- This change is intended to establish a safer baseline for later, separate improvements to pooling, dispatching, patching, and async abstractions.
