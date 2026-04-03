## 1. Correct core lifecycle and async correctness bugs

- [x] 1.1 Update `BaseObject` create flows so created state is only set after successful `OnCreate(...)` completion across all overloads
- [x] 1.2 Fix `AsyncRequestCtrl` so finished requests trigger finish events and errored requests trigger error events
- [x] 1.3 Update `ThreadedTask` terminal behavior so thrown worker exceptions do not leave tasks indefinitely in progress
- [x] 1.4 Fix `CoroutinePool` queued cancellation logic to remove waiting tasks safely without mutating collections during enumeration

## 2. Add regression coverage for corrected runtime contracts

- [x] 2.1 Create a Unity test assembly for core runtime reliability tests if one does not already exist
- [x] 2.2 Add tests covering failed and successful `BaseObject` creation state transitions
- [x] 2.3 Add tests covering `AsyncRequestCtrl` finish and error event routing
- [x] 2.4 Add tests covering `ThreadedTask` fault termination behavior
- [x] 2.5 Add tests covering safe cancellation of queued coroutine tasks

## 3. Validate and document the new reliability baseline

- [x] 3.1 Run the narrowest relevant Unity test command(s) for the new reliability coverage and fix any failures
  Validation result: batch-mode EditMode tests execute and pass in the available local Unity 2022 environment, producing `TestResults/editmode-results.xml` with 7 passing tests. The `CoroutinePool` cancellation test was moved to PlayMode because `DontDestroyOnLoad` is not valid in EditMode. Batch-mode PlayMode execution in the available local environment still reports `No tests were executed` for the PlayMode assembly, so PlayMode verification remains a follow-up validation item for the target Unity version and/or editor test runner rather than a blocker for this change.
- [x] 3.2 Run Unity batch-mode compilation to confirm the project still compiles after the reliability fixes
- [x] 3.3 Update repository documentation where current wording conflicts with the corrected runtime behavior or supported expectations
