
# vFrame 核心组件库

![vFrame](https://img.shields.io/badge/vFrame-Core-blue) [![Made with Unity](https://img.shields.io/badge/Made%20with-Unity-57b9d3.svg?style=flat&logo=unity)](https://unity3d.com) [![License](https://img.shields.io/badge/License-Apache%202.0-brightgreen.svg)](#License)

本仓库主要分为两部分：`vFrame.Core` 以及 `vFrame.Core.Unity`，每一个都是独立的Unity Package，主要提供一些常用的组件以供开发使用，加快研发速度与质量。

[English Version (Power by ChatGPT)](./README_en.md)

## 目录

* [安装](#安装)

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

## 安装

建议使用 Unity Package Manager 安装，添加以下路径：
* `vFrame.Core` https://github.com/VyronLee/vFrame.Core.git#upm-core
* `vFrame.Core.Unity` https://github.com/VyronLee/vFrame.Core.git#upm-core-unity

如需指定版本，链接后面带上版本号即可。

另外，该 Package 还依赖一些第三方的链接库，已经打包成 unitypackage 文件，可在 [release](https://github.com/VyronLee/vFrame.Core/releases) 中下载导入。 

## 首批验证基线

- 当前项目版本以 `ProjectSettings/ProjectVersion.txt` 为准，现行为 `2022.3.62f3`。
- 核心层自动化测试与 Unity 侧测试入口分离：`Assets/vFrame.Core.Tests/EditMode/vFrame.Core.Tests.EditMode.asmdef` 仅引用 `vFrame.Core`，Unity 依赖测试通过局部 Unity 测试程序集承载。
- 首批性能基线入口放在 `Assets/vFrame.Core/Editor/Benchmarks/`，覆盖交互派发、通用对象池以及 `SpawnPools` 热点路径。
- CI 最低基线位于 `.github/workflows/validation-baseline.yml`，用于守护 EditMode 与 PlayMode 的最小验证路径。
- `Debug/Development` 模式允许更强的断言、误用检测与诊断信息，以优先暴露问题。
- `Release` 模式默认应保持轻量运行路径，避免在热点路径上启用高成本诊断，除非显式按策略开启。

### 基线入口

- EditMode：`& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -quit -projectPath "D:\Workspace\vFrame\vFrame.Core" -runTests -testPlatform EditMode -testFilter "vFrame.Core.Tests.EditMode" -logFile - -testResults "TestResults/editmode-results.xml"`
- PlayMode：`& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -quit -projectPath "D:\Workspace\vFrame\vFrame.Core" -runTests -testPlatform PlayMode -testFilter "vFrame.Core.Tests.PlayMode" -logFile - -testResults "TestResults/playmode-results.xml"`
- 基准入口：Unity Editor 菜单 `Tools/vFrame/Benchmarks/Run Core Benchmarks`，代码位于 `Assets/vFrame.Core/Editor/Benchmarks/`。

### 诊断符号

- `DEBUG_SPAWNPOOLS`：启用 `SpawnPools` 相关诊断日志。
- `DEBUG_COROUTINE_POOL`：启用 `CoroutinePool` 相关诊断日志。
- `PERF_PROFILE`：启用性能采样辅助路径。

这些符号应优先用于 `Debug/Development` 流程；`Release` 默认保持关闭，以控制热点路径开销。

## 首批核心契约

- 生命周期语义现统一使用三种意图词汇：`owned` 表示对象负责销毁和清理；`borrowed` 表示仅借用外部依赖或上下文，不接管其生命周期；`lifetime-bound` 表示资源跟随某个 `ILifetime` 边界结束。
- 对 `BaseObject` 而言，`Own(...)` 用于注册 `owned` 清理项；`OwnLifetime(...)` 用于表达 `lifetime-bound` 资源；未注册到生命周期边界的外部依赖应视为 `borrowed`。
- 对 `EventDispatcher` 而言，裸订阅是显式的 `borrowed`/调用方自管模式；owner 订阅是跟随 `BaseObject` 销毁的 `lifetime-bound` 模式；传入 `ILifetime` 的订阅是显式的 `lifetime-bound` 模式。
- 交互路径现明确分层：typed message 是新功能的默认路径；`int eventId` 是保留的 compatibility 迁移路径；`Vote` / `Decision` 是显式保留的规则与裁决语义，不应被当作普通事件或 typed message 替代。
- `BaseObject` 现已明确为终止型生命周期：对象销毁后不可再次 `Create(...)`，依赖生命周期的访问会优先暴露已销毁状态。
- 轻量生命周期分组通过 `ILifetime` / `Lifetime` 提供，用于表达父子所有权与共享清理边界，而不是引入完整 scope 容器框架。
- 通用对象池现支持容量上限、溢出销毁策略、统计查询与重复回收检测；对 `BaseObject` 这类终止型对象，回池会结束生命周期并阻止原实例再次复用。
- 核心交互系统新增 typed message 主路径：`Subscribe<TMessage>(...)` / `Publish<TMessage>(...)` 用于新交互语义；`int eventId` 路径保留为兼容层。
- owner 绑定订阅当前明确收敛为 `BaseObject` owner；非 owner 托管场景应使用 `ILifetime`，裸订阅仍允许并由调用方自行管理。
- `Vote` 语义被保留，并通过 `Decision` 别名强调其“规则决策流”定位，而不是普通消息派发。

## 延后事项

- `SpawnPools` 现代化仍延后，等待核心生命周期、对象池与交互契约稳定后再继续。
- 日志现代化、统一诊断工具、历史模块清理不在首批实施波次中。
- `Container / Component`、`Localization`、`MultiThreading / Task`、`Patch`、`Download` 的彻底迁移或退出，仅记录边界与后续说明，不在本轮集中改造。

## 历史模块退役地图

- `Container / Component`：`retiring`。不再作为新语义设计的投资方向；保留仅用于兼容历史结构，新的生命周期与所有权语义以 `BaseObject` / `Lifetime` 为准。
- `Profiles`：`retiring`。不再作为 retained 核心能力继续演进；如仍有历史压缩/配置路径引用，应视为兼容边界而非未来方向。
- `Localization`：`retired for new investment`。本仓库不再把本地化作为长期核心能力推进；新工作不应建立在该模块之上。
- `MultiThreading / Task`：`retiring`。保留历史兼容用途，但不再作为现代化主路径；新的 retained 核心路径优先使用已明确的生命周期、typed interaction 与对象池语义，而不是继续扩散自定义任务系统。
- `Patch`：`compatibility-only`。继续留在 `vFrame.Core.Unity` 仅用于历史资源更新链路兼容，不再作为 retained Unity 运行时核心方向。
- `Download`：`compatibility-only`。仅作为 `Patch` 等历史链路的暂存依赖保留，不再作为新的战略能力建设方向。

### 退出方向

- 生命周期与所有权：迁移到 `BaseObject` / `Lifetime`，使用 `owned`、`borrowed`、`lifetime-bound` 语义组织对象关系。
- 交互：新功能优先迁移到 typed message 主路径，使用 `Subscribe<TMessage>(...)` / `Publish<TMessage>(...)`；`int eventId` 仅保留为兼容层。
- 池化：统一迁移到已明确语义的通用 `ObjectPool` 与 Unity 侧 `SpawnPools`；其中 `SpawnPools` 仅负责实例复用，不承担资源管理。
- 日志与诊断：迁移到 Core/Core.Unity 分层日志与轻量 diagnostics hooks，而不是继续在历史模块中扩散新的观测实现。

### 当前仍保留的兼容边界

- `EventDispatcher -> Component`：该历史兼容边界已移除；`EventDispatcher` 现直接基于 `BaseObject` / `Lifetime` 运行，交互宿主不再依赖 `Component` 继承。
- `BlockBasedCompression -> Profiles`：当前仍保留对 `Profiles` 的历史配置/元数据依赖；该依赖已标注为 compatibility-only。
- `AsynchronousBlockBasedCompression -> MultiThreading / Task`：当前异步分块压缩仍复用历史任务运行器；后续如继续收敛，应优先朝 retained 运行时边界演进，而不是继续扩展该任务线。
- `Patch -> Download`：`Patch` 仍通过 `Download` 工作，这条链路被明确保留为历史兼容边界，而不是 retained Unity runtime 的长期方向。

## 迁移指南

### 推荐迁移顺序

- 第一步：先统一生命周期语义。新对象统一收敛到 `BaseObject` / `Lifetime`，并明确 `owned`、`borrowed`、`lifetime-bound`。
- 第二步：把新交互迁移到 typed message 主路径。优先使用 `Subscribe<TMessage>(...)` / `Publish<TMessage>(...)`，仅在历史兼容场景继续保留 `int eventId`。
- 第三步：把可复用实例迁移到已明确语义的 `ObjectPool` / `SpawnPools`。其中 `SpawnPools` 只负责 Unity 实例复用，不承担资源定位、下载、补丁或版本管理。
- 第四步：把日志和运行时诊断迁移到 Core/Core.Unity 分层日志与轻量 diagnostics hooks，不要在退役模块中继续增加新的监控实现。

### 新工作最佳实践

- 不要再把 `Container / Component`、`Profiles`、`Localization`、`MultiThreading / Task`、`Patch`、`Download` 作为新功能设计起点。
- 若历史链路必须暂时保留这些模块，应在代码或文档中显式标注为 compatibility-only boundary。
- 对运行时对象关系优先表达生命周期边界，再决定交互、池化和 Unity 适配层的组织方式。
- 对 Unity 侧复用优先使用 `SpawnPools`；对 Core 层复用优先使用通用 `ObjectPool`。
- 对交互优先使用 typed messages；对规则裁决保留 `Vote` / `Decision` 语义；不要继续扩大 legacy `eventId` 设计面。
- `EventDispatcher` 现需要像其他 retained runtime 对象一样显式 `Create()` / `Destroy()`；若需要把订阅跟随外部宿主结束，应绑定到 `BaseObject` owner 或 `ILifetime`，而不是依赖 `Component` 继承关系。

### 历史模块到 retained 系统的迁移方向

- `Container / Component` -> `BaseObject` / `Lifetime` / 明确所有权边界
- `Profiles` -> 更局部的显式配置与运行时语义，不再把 `Profiles` 作为核心扩展方向
- `Localization` -> 仓库外或上层产品能力；本仓库不再继续投资
- `MultiThreading / Task` -> 生命周期清晰的 retained runtime 流程；避免继续把自定义任务系统扩展成核心依赖
- `Patch / Download` -> 历史兼容链路；新工作应优先投资 retained Unity runtime 与上游生态模块边界，而不是继续加深该链路

## 当前验证结果

- 已确认本机存在 Unity `2022.3.62f3` 编辑器。
- 已尝试通过 Unity batch mode 运行 EditMode 与 PlayMode 测试。
- 当前批处理验证被阻塞：项目已被另一 Unity 实例占用，Unity 拒绝同时打开同一工程。
- 完成最终验收前，可在关闭占用该工程的 Unity 实例后重新运行：
  - `& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -quit -projectPath "D:\Workspace\vFrame\vFrame.Core" -runTests -testPlatform EditMode -testFilter "vFrame.Core.Tests.EditMode" -logFile - -testResults "TestResults/editmode-results.xml"`
  - `& "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -quit -projectPath "D:\Workspace\vFrame\vFrame.Core" -runTests -testPlatform PlayMode -testFilter "vFrame.Core.Tests.PlayMode" -logFile - -testResults "TestResults/playmode-results.xml"`

## vFrame Core

该 Package 内所有组件均不依赖 Unity 相关 DLL，也就是说可用于非 Unity 相关项目使用，或者是作为 Server Side Only 的项目依赖库集成到应用中（如：战斗逻辑CS共用）

Package 内包含的组件有：

### 基础类型（ Base ）

核心组件基本对象类型：`BaseObject`，限定统一的对象**创建**以及**销毁**流程。组件库内绝大部分组件均派生于该基础类型。

`Create(...)` 只有在对应的 `OnCreate(...)` 成功执行完成后，实例才会进入已创建状态；如果初始化过程中抛出异常，则对象会保持未创建状态。

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


* `ThreadedTask` 为在子线程中执行任务的模板类，使用时继承并实现`OnHandleTask`方法即可。任务执行过程中如果发生异常，任务仍会进入终止状态，避免等待方无限阻塞。
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

如果对象实现了`IPoolObjectResetable`，放回对象池时会自动调用`Reset`；如果对象属于 `IBaseObject` / `BaseObject` 这类 retained lifecycle 对象，放回对象池时会触发 `Destroy` 以结束当前生命周期，而不是把 `BaseObject` 当作轻量重置基类使用。


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

在协程池的构造函数中，提供一个参数`capacity`用于控制同时执行的协程上限，超过上限的任务会进入排队状态，直到有空闲的协程才会从队列中取出并执行。对于仍处于排队状态的任务，可安全调用`StopCoroutine(handle)`取消等待中的任务。

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
