# vFrame.Core

Unity-free core C# library + Unity extension assembly for the vFrame workspace.

## Purpose

Foundational capability library: lifecycle model (`BaseObject`, `ILifetime`), typed dispatching (`Subscribe<TMessage>/Publish<TMessage>`), object pools, logging, compression/encryption. Split into Unity-free `vFrame.Core` and Unity-dependent `vFrame.Core.Unity`.

## Assemblies & Structure

| Assembly | References | Purpose |
|----------|------------|---------|
| `vFrame.Core` | None | Unity-free core (BaseObject, Dispatcher, ObjectPool, Logger, compression, encryption) |
| `vFrame.Core.Unity` | vFrame.Core | Unity runtime helpers (SpawnPools, CoroutinePool, Downloader, Unity-side async) |
| `vFrame.Core.Tests.EditMode` | vFrame.Core | Unity-free EditMode tests |
| `vFrame.Core.Tests.EditMode.Unity` | vFrame.Core, vFrame.Core.Unity | Unity-dependent EditMode tests |
| `vFrame.Core.Tests.PlayMode` | vFrame.Core, vFrame.Core.Unity | PlayMode tests |

**Directory layout:**
- `Assets/vFrame.Core/Runtime/` - Unity-free core: `Base/`, `Dispatchers/`, `Loggers/`, `ObjectPools/`, `Compression/`, `Encryption/`, `Async/`, `Generic/`, `Utils/`
- `Assets/vFrame.Core.Unity/Runtime/` - Unity extensions: `SpawnPools/`, `Coroutine/`, `Download/`, `Asynchronous/`

## Key Concepts

### BaseObject Lifecycle

```csharp
public class Session : BaseObject<string>
{
    protected override void OnCreate(string name) { /* init */ }
    protected override void OnDestroy() { /* cleanup */ }
}
var session = new Session();
session.Create("Game");
session.Destroy(); // terminal - cannot Create() again
```

**Ownership semantics:** `owned` (call Destroy), `borrowed` (no teardown), `lifetime-bound` (bound to ILifetime).

### Dispatcher (Typed Messaging)

```csharp
public readonly struct PlayerJoined : IEvent { public string Name; }

var dispatcher = new Dispatcher();
dispatcher.Create(); // required - derives from BaseObject
dispatcher.Subscribe<PlayerJoined>(evt => Logger.Info($"Player: {evt.Name}"));
dispatcher.Publish(new PlayerJoined { Name = "Alice" });
dispatcher.Destroy(); // required
```

Four dispatch styles: `Subscribe<TEvent>/Publish`, `Handle<TCommand>/Send`, `HandleRequest<TRequest,TResponse>/Request`, `Listen<TDecision>/Decide`.

### Object Pools

```csharp
var list = ListPool<int>.Shared.Get();
// use list
ListPool<int>.Shared.Return(list);

var pool = new ObjectPool<StringBuilder>();
var sb = pool.Get();
pool.Return(sb);
```

Built-in pools: `ListPool<T>`, `DictionaryPool<TKey,TValue>`, `HashSetPool<T>`, `StackPool<T>`, `QueuePool<T>`, `StringBuilderPool`.

### ILifetime/Lifetime

```csharp
var lifetime = new Lifetime();
lifetime.Add(() => Logger.Info("Cleanup"));
lifetime.Add(new SomeDisposable());
var child = lifetime.CreateChild(); // ends when parent ends
lifetime.Destroy();
```

### Logger

```csharp
Logger.ApplyConfiguration(new LogConfiguration {
    GlobalMinimumLevel = LogLevelDef.Info,
    FileLogPath = "Logs/core.log",
    CaptureStackTrace = true
});
var logger = Logger.GetLogger("MyClass"); // or Logger.GetLogger<T>() for non-static
logger.Info("ready");
```

## Build & Test

### Quick compilation check (dotnet)

```powershell
dotnet build "D:/Workspace/vFrame/vFrame.Core/vFrame.Core.csproj" --no-restore 2>&1
dotnet build "D:/Workspace/vFrame/vFrame.Core/vFrame.Core.Unity.csproj" --no-restore 2>&1
```

### Unity batch mode tests

**Close Unity Editor first.**

```powershell
# EditMode (Unity-free)
& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath "D:\Workspace\vFrame\vFrame.Core" `
  -runTests -testPlatform EditMode `
  -testFilter "vFrame.Core.Tests.EditMode" `
  -logFile - `
  -testResults "TestResults/editmode-results.xml"

# PlayMode
& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" `
  -batchmode -quit `
  -projectPath "D:\Workspace\vFrame\vFrame.Core" `
  -runTests -testPlatform PlayMode `
  -testFilter "vFrame.Core.Tests.PlayMode" `
  -logFile - `
  -testResults "TestResults/playmode-results.xml"
```

## Gotchas (Repo-Specific)

### System type collisions

`vFrame.Core` defines `ArgumentNullException`, `ArgumentException`, `InvalidDataException`, `IndexOutOfRangeException` in its namespace. Use `System.` prefix explicitly:
```csharp
System.ArgumentNullException
System.ArgumentException
```

### ILogger ambiguity

When both `UnityEngine` and `vFrame.Core` are referenced, `ILogger` is ambiguous. Alias:
```csharp
using vFrameCore = vFrame.Core;
// use vFrameCore.ILogger
```

### Logger.GetLogger<T>() on static classes

Fails with CS0718. Use string overload:
```csharp
Logger.GetLogger("MyStaticClass") // not GetLogger<MyStaticClass>()
```

### ISpawnPools location

Requires `using vFrame.Core.Unity;` (not `vFrame.Core`).

### BaseObject is terminal after Destroy()

Cannot `Create()` again after `Destroy()`. Use pools for reuse.

### Dispatcher requires explicit Create/Destroy

Derives from BaseObject; must call `Create()` before use, `Destroy()` when done.

## Retiring Modules

Do not use for new features: `Container/Component`, `Profiles`, `Localization`, `MultiThreading/Task`, `Patch`, `Download` (kept for compatibility only).

## How It's Consumed

- **Direct package reference:** Install via UPM Git URL (`com.vyronlee.vframe.core`, `com.vyronlee.vframe.core.unity`)
- **By higher assemblies:** vFrame.MVVM, vFrame.VFS, vFrame.GameFramework depend on `vFrame.Core`
- **Unity-free usage:** `vFrame.Core` asmdef has no Unity references, can be used in plain C# projects

---

**Workspace conventions** (build, test, cross-package gotchas) live in the workspace-root `.claude/rules/`. This file covers vFrame.Core-specifics.
