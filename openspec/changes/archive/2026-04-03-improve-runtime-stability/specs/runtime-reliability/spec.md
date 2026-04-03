## ADDED Requirements

### Requirement: Base object creation SHALL only succeed after successful initialization
Runtime base objects MUST report themselves as created only after `OnCreate(...)` completes successfully. If initialization throws an exception, the object MUST NOT transition into a created state.

#### Scenario: Create fails during initialization
- **WHEN** a `BaseObject`-derived type throws from `OnCreate(...)`
- **THEN** the instance MUST remain not created

#### Scenario: Create succeeds during initialization
- **WHEN** a `BaseObject`-derived type completes `OnCreate(...)` without error
- **THEN** the instance MUST transition into a created state

### Requirement: Async request controller SHALL route terminal events correctly
The async request controller MUST dispatch finish notifications only for successfully finished requests and MUST dispatch error notifications only for faulted requests.

#### Scenario: Request finishes successfully
- **WHEN** an async request enters the `Finished` terminal state
- **THEN** the controller MUST raise the finish event and MUST NOT raise the error event for that request

#### Scenario: Request fails with error
- **WHEN** an async request enters the `Error` terminal state
- **THEN** the controller MUST raise the error event and MUST NOT raise the finish event for that request

### Requirement: Threaded tasks SHALL terminate on failure
Threaded tasks MUST enter a non-blocking terminal state when task execution throws an exception so callers do not wait indefinitely for completion.

#### Scenario: Background task throws during execution
- **WHEN** a threaded task throws while handling work on a worker thread
- **THEN** the task MUST become terminal instead of remaining indefinitely in progress

#### Scenario: Waiting code observes task termination after failure
- **WHEN** calling code waits on a threaded task that has failed during worker execution
- **THEN** the waiting code MUST be able to observe that the task will not continue running indefinitely

### Requirement: Coroutine queue mutation SHALL be safe during cancellation
Coroutine pool cancellation MUST safely remove queued tasks without relying on collection mutation patterns that can invalidate enumeration.

#### Scenario: Cancel waiting coroutine
- **WHEN** a coroutine handle is cancelled while still waiting in the queue
- **THEN** the coroutine MUST be removed without triggering collection-enumeration errors
