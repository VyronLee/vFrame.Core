
# vFrame 核心组件库

![vFrame](https://img.shields.io/badge/vFrame-Core-blue) [![Made with Unity](https://img.shields.io/badge/Made%20with-Unity-57b9d3.svg?style=flat&logo=unity)](https://unity3d.com) [![License](https://img.shields.io/badge/License-Apache%202.0-brightgreen.svg)](#License)

本仓库主要分为两部分：`vFrame.Core` 以及 `vFrame.Core.Unity`，每一个都是独立的Unity Package，主要提供一些常用的组件以供开发使用，加快研发速度与质量。

[English Version (Power by ChatGPT)](./README_en.md)

## 目录

* [vFrame Core](#vframe-core)
    + [基础类型（ Base ）](#-base-)
    + [压缩（ Compression ）](#-compression-)
    + [加密（ Encryption ）](#-encryption-)
    + [事件派发器（ EventDispatcher ）](#-eventdispatcher-)
    + [多线程任务（ MultiThreading ）](#-multithreading-)
    + [对象池（ ObjectPool ）](#-objectpool-)
    + [工具类（ Utils ）以及 一些通用泛型（ Generic ）](#-utils-generic-)
* [vFrame Core Unity](#vframe-core-unity)
    + [协程池（ CoroutinePool ）](#-coroutinepool-)
    + [游戏物件池（ SpawnPools ）](#-spawnpools-)
    + [下载器（ Downloader ）](#-downloader-)
    + [补丁程序（ Patcher ）](#-patcher-)
* [License](#license)

## vFrame Core

该 Package 内所有组件均不依赖 Unity 相关 DLL，也就是说可用于非 Unity 相关项目使用，或者是作为 Server Side Only 的项目依赖库集成到应用中（如：战斗逻辑CS共用）

Package 内包含的组件有：

### 基础类型（ Base ）

核心组件基本对象类型：`BaseObject`，限定统一的对象**创建**以及**销毁**流程。组件库内绝大部分组件均派生于该基础类型。

`IBaseObject` 继承于接口 `ICreatable` 以及 `IDestroyable`，提供了多种泛型用于指定不同参数个数或类型的创建流程
```csharp
public interface IBaseObject : ICreatable, IDestroyable { }
public interface IBaseObject<in T1> : ICreatable<T1>, IDestroyable { }
public interface IBaseObject<in T1, in T2> : ICreatable<T1, T2>, IDestroyable { }
public interface IBaseObject<in T1, in T2, in T3> : ICreatable<T1, T2, T3>, IDestroyable { }
public interface IBaseObject<in T1, in T2, in T3, in T4> : ICreatable<T1, T2, T3, T4>, IDestroyable { }
public interface IBaseObject<in T1, in T2, in T3, in T4, in T5> : ICreatable<T1, T2, T3, T4, T5>, IDestroyable { }
```

使用方式如下：
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
    inst1:Create();
    inst1:Destroy();

    var inst2 = new TwoArgsClass();
    inst2:Create(123, "abc");
    inst2:Destroy();

    // Use as 'IDisposable'
    using(var inst3 = new ZeroArgsClass()) {
        inst3:Create();
    }
    using(var inst4 = new TwoArgsClass()) {
        inst4:Create(123, "abc");
    }
}

```
也可继承`CreateAbility<T, ...>`以使用提供的`工厂方法`简化对象的创建流程
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

### 压缩（ Compression ）

压缩/解压器集成了4种算法，包括`LZ4`, `LZMA`, `Zlib`以及`ZStd`，提供统一抽象接口以实现压缩、解压功能

```csharp
public interface ICompressor : IDisposable
{
    void Compress(Stream input, Stream output);
    void Compress(Stream input, Stream output, Action<long, long> onProgress);
    void Decompress(Stream input, Stream output);
    void Decompress(Stream input, Stream output, Action<long, long> onProgress);
}
```

`ICompressor` 可从 `CompressorPool` 单例中获取

```csharp
public class CompressorPool : Singleton<CompressorPool>
{
    public ICompressor Rent(CompressorType compressorType, CompressorOptions options = null);
    public void Return(ICompressor compressor);
}
```

由于部分压缩库没有提供多线程的操作支持，为了提升超大文件的压缩、解压效率，实现了一套基于**分块**的压缩组件，支持**同步单线程**或者**异步多线程**的压缩解压功能

同步分块压缩器:
```csharp
public class SynchronousBlockBasedCompression : BlockBasedCompression
{
    public void Compress(Stream input, Stream output, BlockBasedCompressionOptions options, Action<int, int> onProgress = null);
    public void Decompress(Stream input, Stream output, Action<int, int> onProgress);
}
```

异步分块压缩器：
```csharp
public class AsynchronousBlockBasedCompression : BlockBasedCompression
{
    public void SetThreadCount(int count);
    public BlockBasedCompressionRequest CompressAsync(Stream input, Stream output, BlockBasedCompressionOptions options);
    public BlockBasedDecompressionRequest DecompressAsync(Stream input, Stream output) {
}
```

### 加密（ Encryption ）

加密/解密器与压缩器类似，提供统一的加解密操作接口：

```csharp
public interface IEncryptor : IDisposable
{
    void Encrypt(byte[] input, byte[] output, byte[] key, int keyLength);
    void Decrypt(byte[] input, byte[] output, byte[] key, int keyLength);
    void Encrypt(Stream input, Stream output, byte[] key, int keyLength);
    void Decrypt(Stream input, Stream output, byte[] key, int keyLength);
}
```

目前只支持 **XOR加密** 或者 **无加密** 两种类型，可从 `EncryptorPool` 单例中获取对象

```csharp
public class EncryptorPool : Singleton<EncryptorPool>
{
    public IEncryptor Rent(EncryptorType encryptorType);
    public void Return(IEncryptor encryptor);
}
```

### 事件派发器（ EventDispatcher ）

事件派发器是一个中介组件，用于管理和协调事件的订阅与通知。它允许对象订阅特定事件，并在事件发生时通知所有注册的监听器。这种机制解耦了事件的发送者和接收者，增强了软件架构的灵活性和可维护性，常用于实现观察者模式。在用户界面框架、游戏开发和消息系统中尤为常见。

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

接口提供了两种类型事件的订阅，分为“普通事件（Event）”以及“投票事件（Vote）”。**普通事件**派发时，所有的订阅者都会**无条件收到通知**；而**投票事件**派发时，监听队列中的订阅者会**有序收到通知**，当某个订阅者拒绝时（返回false），投票流程则终止，事件派发者会得到相应投票结果。

### 多线程任务（ MultiThreading ）

与`.NET`中的多线程任务`Task`不同的是，本仓库中提供的`Task`类型专门为Unity的协程机制设计，可直接使用`yield return`的方式等待任务的执行完成

* `ITask` 接口设计如下
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

  使用者可继承`Task<TArg>`模板类型，实现`RunTask`接口


* `ThreadedTask` 为在子线程中执行任务的模板类，使用时继承并实现`OnHandleTask`方法即可
  ```csharp
  public abstract class ThreadedTask<TArg> : Task<TArg>
  {
      protected abstract void OnHandleTask(TArg arg);
  }
  ```

* 另外，还提供了一个并行任务执行器`ParallelTaskRunner`，可在指定的并发线程个数下同时执行任务，直到完成，使用方法如下：
  ```csharp
  var contexts = new List<YourThreadState>(2048);
  ParallelTaskRunner<YourThreadState>.Spawn(threadCount: 5)
    .OnHandle(HandleAsyncTask)
    .OnComplete(HandleComplete)
    .OnError(HandleException)
    .Run(contexts);
   ```

### 对象池（ ObjectPool ）

对象池是一种内存优化技术，通过重用已经创建但当前不活跃的对象来减少内存分配和垃圾回收的频率。这种技术对于提高性能和资源利用率尤其有效，特别是在需要频繁创建和销毁对象的场景，如游戏开发或高并发应用中。对象池维护一个可用对象的集合，当需要对象时从池中取出，用完后再归还池中。

要实现一个自定义的对象池，可继承`ObjectPool<T>`以及`IPoolObjectAllocator<T>`（非必须），如下所示为`List`对象池的实现：
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
其中`ListAllocator`主要工作为`List`对象的初始化以及重置逻辑，对于没有特殊初始化、重置要求的对象，可不实现该分配器。

当然，也可以不继承`ObjectPool<T>`模板，直接如下使用：
```csharp
var inst = ObjectPool<YourClass>.Shared.Get();
...
ObjectPool<YourClass>.Shared.Return(inst);
```

如果对象是继承了`IPoolObjectResetable`或者`IBaseObject`，放回对象池时也会自动调用`Reset`或者`Destroy`方法（对于这类情况，可在不实现`IPoolObjectAllocator<T>`的情况下也有对象重置的功能）


此外，该仓库中也提供了一些常用的内置对象池，包括有：

* DictionaryPool
* SortedDictionaryPool
* ListPool
* HashSetPool
* QueuePool
* StackPool
* StringBuilderPool

### 工具类（ Utils ）以及 一些通用泛型（ Generic ）

仓库中也提供了一些常用的工具，比如
* EnumUtils  枚举值相关
* TimeUtils  时间操作相关
* MessageDigestUtils  信息摘要相关

以及一些常用泛型：
* Accessor 属性访问器
* Box 数值盒子封装
* GCFreeAction 无GC回调封装
* RecycleOnDestroy 销毁时自动回收封装
* Singleton 单例泛型

## vFrame Core Unity

该 Package 内组件为 Unity 专用，会依赖部分 Unity 的特性

包含的组件有：

### 协程池（ CoroutinePool ）

协程池是一种用于管理协程实例的技术，类似于对象池。它允许开发者重用协程，从而减少创建和销毁协程的开销。协程池维护一组可用的协程，并控制它们的生命周期，当需要执行新的异步任务时，从池中获取一个协程来使用，任务完成后协程被重置并返回池中

```csharp
public class CoroutinePool
{
    public CoroutinePool(string name = null, int capacity = int.MaxValue);
    public int StartCoroutine(IEnumerator task);
    public void StopCoroutine(int handle);
    public void Destroy();
}
```

在协程池的构造函数中，提供一个参数`capacity`用于控制同时执行的协程上限，超过上限的任务会进入排队状态，直到有空闲的协程才会从队列中取出并执行

### 游戏物件池（ SpawnPools ）

游戏物件池是”对象池”的一种特化，旨在重用 Unity 中的 GameObject 对象。接口如下：
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

物件池支持**同步生成**、**异步生成**以及**预先生成**三种模式，并且支持构造时传入`IGameObjectLoaderFactory`来自定义对象生成逻辑，支持设置`SpawnPoolsSettings`来控制每种GameObject对象池的最大容量、生命周期以及GC频率

### 下载器（ Downloader ）

下载器是游戏内很常用的功能，一般用于下载CDN上的文件到本地。该下载器提供有如下功能：
1. 添加下载任务
2. 移除下载任务
3. 暂停下载
4. 恢复下载
5. 获取当前下载速度
6. 下载开始、失败、更新、完成回调

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

### 补丁程序（ Patcher ）

补丁程序是现代手游都得具备的一个基本能力，它主要用于更新游戏功能以及修复游戏BUG。

该补丁程序定义了**补丁清单**文件的格式，集成有**版本检测**、**补丁文件自动对比下载**、**下载文件哈希校验**、**失败文件自动重试**以及**各进度事件通知**等功能

接口定义如下：
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