# vFrame Core Component Library

![vFrame](https://img.shields.io/badge/vFrame-Core-blue) [![Made with Unity](https://img.shields.io/badge/Made%20with-Unity-57b9d3.svg?style=flat&logo=unity)](https://unity3d.com) [![License](https://img.shields.io/badge/License-Apache%202.0-brightgreen.svg)](#License)

This repository is mainly divided into two parts: `vFrame.Core` and `vFrame.Core.Unity`, each of which is an independent Unity Package, mainly providing some commonly used components for development use, to speed up research and development speed and quality.

## Table Of Content

* [Installation](#Installation)

* [vFrame Core](#vframe-core)
    + [Base Types](#-base-)
    + [Compression](#-compression-)
    + [Encryption](#-encryption-)
    + [Event Dispatcher](#-eventdispatcher-)
    + [Multi-threading Tasks](#-multithreading-)
    + [Object Pool](#-objectpool-)
    + [Utility Classes and Some Common Generics](#-utils-generic-)
* [vFrame Core Unity](#vframe-core-unity)
    + [Coroutine Pool](#-coroutinepool-)
    + [Game Object Pool](#-spawnpools-)
    + [Downloader](#-downloader-)
    + [Patcher](#-patcher-)
* [License](#license)

## Installation

It is recommended to install using the Unity Package Manager by adding the following paths:

- `vFrame.Core` https://github.com/VyronLee/vFrame.Core.git#upm-core
- `vFrame.Core.Unity` https://github.com/VyronLee/vFrame.Core.git#upm-core-unity

If you need to specify a version, just add the version number after the link.

Moreover, this Package relies on certain external libraries that have been compiled into a unitypackage file. You can download and import this file from the [release](https://github.com/VyronLee/vFrame.Core/releases) page on GitHub.

## First-Wave Validation Baseline

- The effective project version is taken from `ProjectSettings/ProjectVersion.txt`, which is currently `2022.3.62f3`.
- Core-layer automated tests and Unity-side test entry paths are intentionally split: `Assets/vFrame.Core.Tests/EditMode/vFrame.Core.Tests.EditMode.asmdef` now references only `vFrame.Core`, while Unity-dependent coverage lives in a narrower Unity-scoped test assembly.
- The initial performance baseline entry points live under `Assets/vFrame.Core/Editor/Benchmarks/` and cover interaction dispatch, generic object-pool, and `SpawnPools` hot paths.
- The minimum CI guardrail lives in `.github/workflows/validation-baseline.yml` and protects the first-wave EditMode and PlayMode validation paths.
- `Debug/Development` mode may spend more on assertions, misuse detection, and diagnostics to improve issue discovery.
- `Release` mode should keep default runtime behavior lightweight and avoid diagnostics-heavy overhead on hot paths unless explicitly enabled by policy.

### Baseline Entry Points

- EditMode: `& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -quit -projectPath "D:\Workspace\vFrame\vFrame.Core" -runTests -testPlatform EditMode -testFilter "vFrame.Core.Tests.EditMode" -logFile - -testResults "TestResults/editmode-results.xml"`
- PlayMode: `& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -quit -projectPath "D:\Workspace\vFrame\vFrame.Core" -runTests -testPlatform PlayMode -testFilter "vFrame.Core.Tests.PlayMode" -logFile - -testResults "TestResults/playmode-results.xml"`
- Benchmarks: Unity Editor menu `Tools/vFrame/Benchmarks/Run Core Benchmarks`, with source under `Assets/vFrame.Core/Editor/Benchmarks/`.

### Diagnostics Symbols

- `DEBUG_SPAWNPOOLS`: enables `SpawnPools` diagnostics logging.
- `DEBUG_COROUTINE_POOL`: enables `CoroutinePool` diagnostics logging.
- `PERF_PROFILE`: enables performance profiling helper paths.

These symbols should primarily be enabled in `Debug/Development` flows; `Release` keeps them off by default to avoid extra hot-path overhead.

## First-Wave Core Contracts

- Lifecycle intent now uses three explicit terms: `owned` means the object is responsible for teardown, `borrowed` means the dependency is used without taking over its lifecycle, and `lifetime-bound` means the resource ends with a specific `ILifetime` boundary.
- In `BaseObject`, `Own(...)` registers `owned` cleanup, `OwnLifetime(...)` expresses `lifetime-bound` resources, and dependencies not registered to a lifecycle boundary should be treated as `borrowed`.
- In `EventDispatcher`, bare subscriptions are the explicit `borrowed` / caller-managed mode, owner subscriptions are `lifetime-bound` to a `BaseObject`, and `ILifetime` subscriptions are explicitly `lifetime-bound` to the supplied lifetime.
- Interaction paths are now intentionally split: typed messages are the default path for new work, `int eventId` remains a retained compatibility migration path, and `Vote` / `Decision` remain explicit rule and decision semantics rather than ordinary event or typed-message dispatch.
- `BaseObject` now acts as a terminal lifecycle primitive: once destroyed, an instance cannot be `Create(...)`-ed again, and lifecycle-dependent access reports the destroyed state first.
- Lightweight ownership and grouped cleanup are exposed through `ILifetime` / `Lifetime`, which provide parent-child and shared cleanup boundaries without introducing a full scope framework.
- Generic object pools now support capacity limits, overflow destruction policy, statistics queries, and duplicate-return detection; terminal lifecycle objects such as `BaseObject` instances are ended on return and are not reused as the same live instance.
- The core interaction system now has a typed-message primary path through `Subscribe<TMessage>(...)` / `Publish<TMessage>(...)`; the old `int eventId` path remains as a compatibility layer.
- Owner-bound typed subscriptions are intentionally narrowed to `BaseObject` owners; unmanaged cases should use `ILifetime`, while bare subscriptions remain explicitly caller-managed.
- Vote semantics are retained and clarified through `Decision` aliases to emphasize their rule/approval-flow role rather than treating them as ordinary dispatch.

## Deferred Systems

- `SpawnPools` modernization remains deferred until the lifecycle, pooling, and interaction foundations stabilize.
- Logging modernization, unified diagnostics tooling, and broad historical module cleanup are not part of the first implementation wave.
- `Container / Component`, `Localization`, `MultiThreading / Task`, `Patch`, and `Download` are only documented for later migration or exit-path work in this wave.

## Retirement Map For Historical Modules

- `Container / Component`: `retiring`. It is no longer a strategic direction for new semantics; any remaining usage is compatibility-oriented, while new lifecycle and ownership work is centered on `BaseObject` / `Lifetime`.
- `Profiles`: `retiring`. It is no longer a retained core investment area; if older compression or configuration paths still reference it, treat that as a compatibility boundary rather than future direction.
- `Localization`: `retired for new investment`. This repository no longer treats localization as a long-term core capability, and new work should not build on it.
- `MultiThreading / Task`: `retiring`. Historical compatibility remains possible, but it is no longer the modernization path; retained runtime work should prefer the clarified lifecycle, typed interaction, and pooling directions instead of expanding the custom task line.
- `Patch`: `compatibility-only`. It remains in `vFrame.Core.Unity` only to support historical update pipelines and is no longer a retained Unity runtime investment area.
- `Download`: `compatibility-only`. It remains only as a temporary dependency for historical flows such as `Patch`, not as a strategic capability for new work.

### Exit Direction

- Lifecycle and ownership: move toward `BaseObject` / `Lifetime` and organize relationships with `owned`, `borrowed`, and `lifetime-bound` intent.
- Interaction: migrate new work toward the typed-message primary path through `Subscribe<TMessage>(...)` / `Publish<TMessage>(...)`; keep `int eventId` only as a compatibility layer.
- Pooling: move toward the clarified generic `ObjectPool` semantics and Unity-side `SpawnPools`, where `SpawnPools` is strictly for instance reuse rather than resource ownership.
- Logging and diagnostics: move toward the Core/Core.Unity logging split and lightweight diagnostics hooks instead of expanding observability inside retiring modules.

### Remaining Compatibility Boundaries

- `EventDispatcher -> Component`: this historical compatibility boundary has been removed; `EventDispatcher` now runs directly on `BaseObject` / `Lifetime`, so retained interaction hosting no longer depends on `Component` inheritance.
- `BlockBasedCompression -> Profiles`: the block-based compression path still carries a historical `Profiles` dependency for older metadata/configuration behavior; it is now explicitly marked as compatibility-only.
- `AsynchronousBlockBasedCompression -> MultiThreading / Task`: async block-based compression still reuses the historical task runner; if this area evolves further, it should move toward retained runtime boundaries instead of expanding the legacy task line.
- `Patch -> Download`: `Patch` still works through `Download`; this chain is now explicitly treated as a historical compatibility boundary rather than a retained Unity runtime direction.

## Migration Guide

### Recommended Adoption Order

- Step 1: normalize lifecycle semantics first. Move new objects onto `BaseObject` / `Lifetime` and make `owned`, `borrowed`, and `lifetime-bound` intent explicit.
- Step 2: move new interaction work onto the typed-message primary path. Prefer `Subscribe<TMessage>(...)` / `Publish<TMessage>(...)`, and keep `int eventId` only for legacy compatibility.
- Step 3: move reusable instances onto the clarified `ObjectPool` / `SpawnPools` semantics. `SpawnPools` is only for Unity instance reuse and should not absorb resource location, download, patching, or version ownership.
- Step 4: move logging and runtime inspection onto the Core/Core.Unity logging split and lightweight diagnostics hooks instead of adding new observability behavior inside retiring modules.

### Best Practices For New Work

- Do not use `Container / Component`, `Profiles`, `Localization`, `MultiThreading / Task`, `Patch`, or `Download` as the starting point for new design work.
- If a historical path must remain temporarily, mark it explicitly as a compatibility-only boundary in code or docs.
- Establish lifecycle ownership boundaries first, then layer interaction, pooling, and Unity adaptation on top of them.
- Prefer generic `ObjectPool` for core reuse and `SpawnPools` for Unity-side instance reuse.
- Prefer typed messages for new interaction semantics, keep `Vote` / `Decision` for rule or approval flows, and avoid expanding the legacy `eventId` surface.
- `EventDispatcher` now follows the same explicit `Create()` / `Destroy()` lifecycle as other retained runtime objects; bind subscriptions to a `BaseObject` owner or `ILifetime` when they should end with an external host.

### Historical Module To Retained-System Direction

- `Container / Component` -> `BaseObject` / `Lifetime` / explicit ownership boundaries
- `Profiles` -> narrower explicit configuration and runtime semantics rather than continuing `Profiles` as a core extension point
- `Localization` -> out-of-repo or upper-layer product capability; this repository is no longer investing in it
- `MultiThreading / Task` -> retained runtime flows with clear lifecycle ownership instead of continued expansion of the historical task line
- `Patch / Download` -> historical compatibility chain only; new work should favor retained Unity runtime boundaries and upstream ecosystem responsibilities instead of deepening this path

## Current Validation Result

- Unity `2022.3.62f3` is available on this machine.
- Batch-mode EditMode and PlayMode test runs were attempted.
- Final batch validation is currently blocked because another Unity instance already has this project open, and Unity refuses concurrent access to the same project.
- After closing the other Unity instance, rerun:
  - `& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -quit -projectPath "D:\Workspace\vFrame\vFrame.Core" -runTests -testPlatform EditMode -testFilter "vFrame.Core.Tests.EditMode" -logFile - -testResults "TestResults/editmode-results.xml"`
  - `& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -quit -projectPath "D:\Workspace\vFrame\vFrame.Core" -runTests -testPlatform PlayMode -testFilter "vFrame.Core.Tests.PlayMode" -logFile - -testResults "TestResults/playmode-results.xml"`

## vFrame Core

All components within this Package do not depend on Unity-related DLLs, meaning they can be used for non-Unity related projects, or integrated into applications as a Server Side Only project dependency library (e.g., shared CS for combat logic).

Components included in the Package are:

### Base Types

Core component basic object type: `BaseObject`, defining a uniform object **creation** and **destruction** process. Most components in the component library are derived from this basic type.

`IBaseObject` inherits from the interfaces `ICreatable` and `IDestroyable`, providing a variety of generics for specifying different numbers or types of creation processes.
```csharp
public interface IBaseObject : ICreatable, IDestroyable { }
public interface IBaseObject<in T1> : ICreatable<T1>, IDestroyable { }
public interface IBaseObject<in T1, in T2> : ICreatable<T1, T2>, IDestroyable { }
public interface IBaseObject<in T1, in T2, in T3> : ICreatable<T1, T2, T3>, IDestroyable { }
public interface IBaseObject<in T1, in T2, in T3, in T4> : ICreatable<T1, T2, T3, T4>, IDestroyable { }
public interface IBaseObject<in T1, in T2, in T3, in T4, in T5> : ICreatable<T1, T2, T3, T4, T5>, IDestroyable { }
```

Usage is as follows:
```csharp
public class ZeroArgsClass : BaseObject
{
    protected override void OnCreate() {}
    protected override void OnDestroy() {}
}

public class TwoArgsClass : BaseObject<int, string>
{
    protected override void OnCreate(int arg1, string arg2) {}
    protected override void OnDestroy() {}
}

public static void Main() {
    var inst1 = new ZeroArgsClass();
    inst1.Create();
    inst1.Destroy();

    var inst2 = new TwoArgsClass();
    inst2.Create(123, "abc");
    inst2.Destroy();

    // Use as 'IDisposable'
    using(var inst3 = new ZeroArgsClass()) {
        inst3.Create();
    }
    using(var inst4 = new TwoArgsClass()) {
        inst4.Create(123, "abc");
    }
}
```
You can also inherit `CreateAbility<T, ...>` to use the provided `factory method` to simplify the object creation process.
```csharp
public sealed class YourAwesomeClass : CreateAbility<YourAwesomeClass, int, string>
{
    protected override void OnCreate(int arg1, string arg2) {}
    protected override void OnDestroy() {}
}

public static void Main() {
    using(var awesome = YourAwesomeClass.Create(123, "abc")) {
        Debug.Log("Bravo!");
    }
}
```

### Compression

The compressor integrates four algorithms, including `LZ4`, `LZMA`, `Zlib`, and `ZStd`, providing a unified abstract interface to implement compression and decompression functions.

```csharp
public interface ICompressor : IDisposable
{
    void Compress(Stream input, Stream output);
    void Compress(Stream input, Stream output, Action<long, long> onProgress);
    void Decompress(Stream input, Stream output);
    void Decompress(Stream input, Stream output, Action<long, long> onProgress);
}
```

`ICompressor` can be obtained from the `CompressorPool` singleton.

```csharp
public class CompressorPool : Singleton<CompressorPool>
{
    public ICompressor Rent(CompressorType compressorType, CompressorOptions options = null);
    public void Return(ICompressor compressor);
}
```

Since some compression libraries do not support multi-threaded operations, to improve the efficiency of compressing and decompressing large files, a set of block-based compression components has been implemented, supporting **synchronous single-thread** or **asynchronous multi-thread** compression and decompression functions.

Synchronous block-based compressor:
```csharp
public class SynchronousBlockBasedCompression : BlockBasedCompression
{
    public void Compress(Stream input, Stream output, BlockBasedCompressionOptions options, Action<int, int> onProgress = null);
    public void Decompress(Stream input, Stream output, Action<int, int> onProgress);
}
```

Asynchronous block-based compressor:
```csharp
public class AsynchronousBlockBasedCompression : BlockBasedCompression
{
    public void SetThreadCount(int count);
    public BlockBasedCompressionRequest CompressAsync(Stream input, Stream output, BlockBasedCompressionOptions options);
    public BlockBasedDecompressionRequest DecompressAsync(Stream input, Stream output);
}
```

### Encryption

The encryptor/decryptor, similar to the compressor, provides a unified encryption and decryption operation interface:

```csharp
public interface IEncryptor : IDisposable
{
    void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength);
    void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength);
    void Encrypt(Stream input, Stream output, byte[] key, int keyLength);
    ...

The encryptor/decryptor, similar to the compressor, provides a unified encryption and decryption operation interface:

```csharp
public interface IEncryptor : IDisposable
{
    void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength);
    void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength);
    void Encrypt(Stream input, Stream output, byte[] key, int keyLength);
    void Decrypt(Stream input, Stream output, byte[] key, int keyLength);
}
```

Currently, it only supports **XOR encryption** or **no encryption** types, and objects can be obtained from the `EncryptorPool` singleton.

```csharp
public class EncryptorPool : Singleton<EncryptorPool>
{
    public IEncryptor Rent(EncryptorType encryptorType);
    public void Return(IEncryptor encryptor);
}
```

### Event Dispatcher

The event dispatcher is a mediating component used to manage and coordinate the subscription and notification of events. It allows objects to subscribe to specific events and notifies all registered listeners when an event occurs. This mechanism decouples event senders and receivers, enhancing the flexibility and maintainability of software architecture, commonly used to implement the observer pattern. It is particularly common in user interface frameworks, game development, and messaging systems.

```csharp
public interface IEventDispatcher
{
    uint AddEventListener(IEventListener listener, int eventId);
    uint AddEventListener(Action<IEvent> listener, int eventId);
    IEventListener RemoveEventListener(uint handle);
    void DispatchEvent(int eventId);
    void DispatchEvent(int eventId, object context);

    uint AddVoteListener(IVoteListener listener, int voteId);
    uint AddVoteListener(Func<IVote, bool> voteDelegate, int voteId);
    IVoteListener RemoveVoteListener(uint handle);
    bool DispatchVote(int voteId, object context);
    bool DispatchVote(int voteId);

    void RemoveAllListeners();
    int GetEventExecutorCount();
    int GetVoteExecutorCount();
}
```

The interface provides two types of event subscriptions, divided into "normal events (Event)" and "voting events (Vote)". When **normal events** are dispatched, all subscribers are notified unconditionally; when **voting events** are dispatched, subscribers in the listener queue are notified in order, and if a subscriber refuses (returns false), the voting process is terminated, and the event dispatcher receives the corresponding voting result.

### Multi-threading Tasks

Different from the multi-threading tasks `Task` in `.NET`, the `Task` type provided in this repository is specifically designed for Unity's coroutine mechanism and can be awaited with `yield return` to wait for the task to complete.

* The `ITask` interface is designed as follows
  ```csharp
  public interface IAsync : IEnumerator
  {
      bool IsDone { get; }
      float Progress { get; }
  }
  
  public interface ITask : IAsync { }
  
  public abstract class Task<TArg> : BaseObject<TArg>, ITask
  {
      public abstract void RunTask();
  }
  ```

  Users can inherit the `Task<TArg>` template type and implement the `RunTask` interface.


* `ThreadedTask` is a template class for executing tasks in a child thread. To use it, inherit and implement the `OnHandleTask` method.
  ```csharp
  public abstract class ThreadedTask<TArg> : Task<TArg>
  {
      protected abstract void OnHandleTask(TArg arg);
  }
  ```

* In addition, a parallel task executor `ParallelTaskRunner` is provided, which can execute tasks concurrently until completion under a specified number of concurrent threads. The usage is as follows:
  ```csharp
  var contexts = new List<YourThreadState>(2048);
  ParallelTaskRunner<YourThreadState>.Spawn(threadCount: 5)
    .OnHandle(HandleAsyncTask)
    .OnComplete(HandleComplete)
    .OnError(HandleException)
    .Run(contexts);
  ```

### Object Pool

The object pool is a memory optimization technique that reduces the frequency of memory allocation and garbage collection by reusing already created but currently inactive objects. This technique is particularly effective in improving performance and resource utilization, especially in scenarios where objects need to be created and destroyed frequently, such as game development or high-concurrency applications. The object pool maintains a collection of available objects, which are taken out of the pool when needed and returned to the pool after use.

To implement a custom object pool, you can inherit `ObjectPool<T>` and `IPoolObjectAllocator<T>` (not mandatory). The following shows the implementation of an object pool for `List` objects:
```csharp
public class ListAllocator<T> : IPoolObjectAllocator<List<T>>
{
    public int PresetLength = 64;

    public List<T> Alloc() {
        return new List<T>(PresetLength);
    }

    public void Reset(List<T> obj) {
        obj.Clear();
    }
}

public class ListPool<T> : ObjectPool<List<T>, ListAllocator<T>>
{
}
```
`ListAllocator` mainly works for the initialization and reset logic of `List` objects. For objects that do not have special initialization and reset requirements, this allocator does not need to be implemented.

Of course, you can also use it directly without inheriting the `ObjectPool<T>` template, as shown below:
```csharp
var inst = ObjectPool<YourClass>.Shared.Get();
...
ObjectPool<YourClass>.Shared.Return(inst);
```

If an object implements `IPoolObjectResetable`, `Reset` is called automatically when the object is returned to the pool. If an object is an `IBaseObject` / `BaseObject` retained lifecycle object, returning it to the pool will call `Destroy` to end that lifecycle instead of treating `BaseObject` as a lightweight reset helper.

In addition, this repository also provides some commonly used built-in object pools, including:

* DictionaryPool
* SortedDictionaryPool
* ListPool
* HashSetPool
* QueuePool
* StackPool
* StringBuilderPool

### Utility Classes and Some Common Generics

The repository also provides some commonly used tools, such as
* EnumUtils related to enumeration values
* TimeUtils related to time operations
* MessageDigestUtils related to message digests

And some common generics:
* Accessor property accessor
* Box value box encapsulation
* GCFreeAction GC-free callback encapsulation
* RecycleOnDestroy automatic recycling on destruction
* Singleton singleton generic

## vFrame Core Unity

The components within this Package are specific to Unity and will depend on some features of Unity.

Components included are:

### Coroutine Pool

The coroutine pool is a technique for managing coroutine instances, similar to an object pool. It allows developers to reuse coroutines, thereby reducing the overhead of creating and destroying coroutines. The coroutine pool maintains a group of available coroutines and controls their lifecycle. When a new asynchronous task is needed, a coroutine is obtained from the pool for use, and the coroutine is reset and returned to the pool after the task is completed.

```csharp
public class CoroutinePool
{
    public CoroutinePool(string name = null, int capacity = int.MaxValue);
    public int StartCoroutine(IEnumerator task);
    public void StopCoroutine(int handle);
    public void Destroy();
}
```

In the constructor of the coroutine pool, a parameter `capacity` is provided to control the maximum number of coroutines that can be executed simultaneously. Tasks exceeding the limit will be queued until a free coroutine is available to be taken out and executed.

### Game Object Pool

The game object pool is a specialization of "object pool", aimed at reusing GameObject objects in Unity. The interface is as follows:
```csharp
public interface ISpawnPools
{
    GameObject Spawn(string assetPath, Transform parent = null);
    ILoadAsyncRequest SpawnAsync(string assetPath, Transform parent = null);
    void Recycle(GameObject obj);
    IPreloadAsyncRequest PreloadAsync(string[] assetPaths);
    void Update();
}
```

The object pool supports **synchronous generation**, **asynchronous generation**, and **pre-generation** modes, and supports passing `IGameObjectLoaderFactory` at construction to customize object generation logic, setting `SpawnPoolsSettings` to control the maximum capacity, lifecycle, and GC frequency of each GameObject object pool.

### Downloader

The downloader is a very common feature in games, generally used to download files from the CDN to the local system. The downloader provides the following functions:
1. Add download tasks
2. Remove download tasks
3. Pause download
4. Resume download
5. Get current download speed
6. Callbacks for download start, failure, update, and completion

```csharp
public class Downloader : MonoBehaviour
{
    public DownloadTask AddDownload(string downloadPath, string downloadUrl, object userData = null);
    public void RemoveDownload(int taskId);
    public void RemoveAllDownloads();
    public void Pause();
    public void Resume();
    public float Speed { get; }
    public event Action<DownloadEventArgs> DownloadStart;
    public event Action<DownloadEventArgs> DownloadUpdate;
    public event Action<DownloadEventArgs> DownloadSuccess;
    public event Action<DownloadEventArgs> DownloadFailure;
}
```

### Patcher

The patcher is a basic capability that modern mobile games must possess, mainly used for updating game features and fixing game bugs.

The patcher defines the format of the **patch manifest** file and integrates **version checking**, **automatic comparison downloading of patch files**, **download file hash verification**, **automatic retries for failed files**, and various progress event notifications.

Interface definition is as follows:
```csharp
public class Patcher
{
    public Patcher(PatchOptions options);
    public string CdnUrl { get; set; }
    public string DownloadUrl { get; }
    public int HashNum { get; }
    public int HashTotal { get; }
    public string EngineVersion { get; }
    public string AssetsVersion { get; }
    public string RemoteEngineVersion { get; }
    public string RemoteAssetsVersion { get; }
    public ulong TotalSize { get; }
    public float DownloadSpeed { get; }
    public bool IsPaused { get; }
    public UpdateState UpdateState { get; }
    public void Pause();
    public void Resume();
    public void Stop();
    public void Release();
    public void CheckUpdate();
    public void StartUpdate();
    public event Action<UpdateEvent> OnUpdateEvent;
}
```

## License

[Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0)
