# vFrame.Core Performance Notes

Allocation-profile notes for hot paths that are sensitive to garbage generation.
Characterized by the EditMode tests in `Assets/vFrame.Core.Tests/EditMode/`.

## Dispatcher & Logger allocation profile

### Dispatcher — `Publish<TEvent>(in TEvent)`

The typed dispatch hot path is **zero-box** for value-type events:

- `Publish<TEvent>(in TEvent payload)` is generic and takes the payload by `in` (by-ref),
  so a `struct` event is never boxed on the way in.
- Subscribers receive the payload typed (`Action<TEvent>`), so delivery is also box-free.
- Characterized by `DispatcherStructEventTests` (correctness + steady-state GC stability)
  and `DispatcherZeroGcTests` (class-event steady state).

**Residual allocation surface (opt-in):** when interceptors are registered, the interceptor
path boxes the payload (`IEvent eventData = payload`) to hand it to non-generic interceptor
callbacks. This only runs while interceptors are present — avoid registering interceptors on
ultra-hot event streams if you are allocation-sensitive.

### Logger — `[InterpolatedStringHandler]` overloads

Every `Logger.Trace/Debug/Info/Warning/Error/Fatal($"...")` call routes through an
`[InterpolatedStringHandler]`:

- When the message's level is below the global `Logger.LogLevel` gate, the handler
  constructor short-circuits — no `StringBuilder` is leased and no `AppendFormatted` runs,
  so a rejected log constructs **zero string**.
- Characterized by `LoggerZeroGcTests` (disabled level produces no buffered entries).

**Rule of thumb:** on hot paths always use the interpolated overloads (`Logger.Info($"...")`),
never `string.Format` or manual concatenation — only the interpolated form can be elided by
the handler when the level is gated out.
