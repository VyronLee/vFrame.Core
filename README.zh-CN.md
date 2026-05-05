[English](README.md) | [简体中文](README.zh-CN.md)

# vFrame.Core

`vFrame.Core` 是 vFrame 工作区中的基础能力库，按程序集拆分为不依赖 Unity 的 `vFrame.Core` 和依赖 Unity 的 `vFrame.Core.Unity`。它提供统一生命周期、类型化消息派发、对象池、日志、压缩加密，以及 Unity 侧的协程池、实例复用与下载辅助能力。

## 特性

- 核心程序集 `vFrame.Core` 不依赖 Unity，可用于纯 C# 场景
- 以 `BaseObject`、`ILifetime`、`Lifetime` 为中心的统一生命周期模型
- `Dispatcher` 提供 Event、Command、Request、Decision 四种类型化交互模式
- 提供 `ObjectPool<T>`、`ListPool<T>`、`StringBuilderPool` 等对象池能力
- 内置 `Logger`、`ILogger`、`LogConfiguration` 等日志基础设施
- 提供 `CompressorPool`、`SynchronousBlockBasedCompression`、`EncryptorPool` 等压缩加密入口
- Unity 扩展程序集提供 `SpawnPools`、`CoroutinePool`、`Downloader` 等运行时辅助组件
- 使用独立 asmdef 划分运行时与测试程序集

## 前置要求

- Unity `2022.3.62f3` 或更高版本
- 如果只使用核心能力，可只引入 `vFrame.Core`
- 如果需要 Unity 运行时扩展，再额外引入 `vFrame.Core.Unity`
- 包自身的 `package.json` 最低 Unity 字段为 `2018.4`，但本仓库当前实际开发与验证版本为 `2022.3.62f3`

## 安装

### 通过 UPM Git URL 安装

本仓库发布两个 UPM 包：

- `Assets/vFrame.Core`
- `Assets/vFrame.Core.Unity`

在项目的 `Packages/manifest.json` 中添加：

```json
{
  "dependencies": {
    "com.vyronlee.vframe.core": "https://github.com/VyronLee/vFrame.Core.git?path=/Assets/vFrame.Core",
    "com.vyronlee.vframe.core.unity": "https://github.com/VyronLee/vFrame.Core.git?path=/Assets/vFrame.Core.Unity"
  }
}
```

也可以通过 Unity Package Manager 菜单 `Window > Package Manager > Add package from git URL...` 手动添加。

### 版本信息

- `com.vyronlee.vframe.core` `1.1.1`
- `com.vyronlee.vframe.core.unity` `1.1.1`
- License: `Apache-2.0`

如果需要锁定标签版本，可在 URL 末尾追加 `#tag`，例如：

```text
https://github.com/VyronLee/vFrame.Core.git?path=/Assets/vFrame.Core#v1.1.5
```

## 快速开始

### 1. 创建一个带生命周期边界的对象

```csharp
using vFrame.Core;

public class Session : BaseObject<string>
{
    public string Name { get; private set; }

    protected override void OnCreate(string name)
    {
        Name = name;
        OwnLifetime(() => Logger.Info($"Session {Name} disposed"));
    }

    protected override void OnDestroy()
    {
    }
}

var session = new Session();
session.Create("Game");
session.Destroy();
```

### 2. 使用 `Dispatcher` 发布类型化事件

```csharp
using vFrame.Core;

public readonly struct PlayerJoined : IEvent
{
    public PlayerJoined(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

var dispatcher = new Dispatcher();
dispatcher.Create();

var subscription = dispatcher.Subscribe<PlayerJoined>(evt =>
{
    Logger.Info($"Player joined: {evt.Name}");
});

dispatcher.Publish(new PlayerJoined("Alice"));
dispatcher.Unsubscribe(subscription);
dispatcher.Destroy();
```

### 3. 使用对象池与集合池

```csharp
using System.Text;
using vFrame.Core;

var list = ListPool<int>.Shared.Get();
list.Add(1);
list.Add(2);
ListPool<int>.Shared.Return(list);

var builders = new ObjectPool<StringBuilder>();
var builder = builders.Get();
builder.Append("vFrame.Core");
builders.Return(builder);
```

## 用法

### 生命周期与资源所有权

`BaseObject` 是本库最核心的生命周期基类：

- `Create(...)` 只会成功进入一次已创建状态
- `Destroy()` 是终态操作，销毁后不能再次 `Create()`
- `Own(...)` 表示当前对象拥有该资源并负责销毁
- `OwnLifetime(...)` 表示资源绑定到当前生命周期边界结束

```csharp
using vFrame.Core;

public class Parent : BaseObject
{
    private readonly Lifetime _lifetime = new Lifetime();

    protected override void OnCreate()
    {
        var child = new Child();
        child.Create();

        Own(child);
        OwnLifetime(_lifetime);
    }

    protected override void OnDestroy()
    {
    }
}

public class Child : BaseObject
{
    protected override void OnCreate()
    {
    }

    protected override void OnDestroy()
    {
    }
}
```

这里可以用三种语义理解资源关系：

- `owned`：当前对象负责释放
- `borrowed`：当前对象使用该资源，但不接管生命周期
- `lifetime-bound`：资源跟随某个 `ILifetime` 边界结束

### 类型化消息派发

`Dispatcher` 实现了 `IDispatcher`，聚合了四类接口：

- `IEventDispatcher`：`Subscribe<TEvent>()` / `Publish<TEvent>()`
- `ICommandDispatcher`：`Handle<TCommand>()` / `Send<TCommand>()`
- `IRequestDispatcher`：`HandleRequest<TRequest, TResponse>()` / `Request<TRequest, TResponse>()`
- `IDecisionDispatcher`：`Listen<TDecision>()` / `Decide<TDecision>()`

示例：请求/响应模式。

```csharp
using vFrame.Core;

public readonly struct SumRequest : IRequest<int>
{
    public SumRequest(int left, int right)
    {
        Left = left;
        Right = right;
    }

    public int Left { get; }
    public int Right { get; }
}

var dispatcher = new Dispatcher();
dispatcher.Create();

dispatcher.HandleRequest<SumRequest, int>(request => request.Left + request.Right);
var result = dispatcher.Request<SumRequest, int>(new SumRequest(2, 3));

dispatcher.Destroy();
```

新代码优先使用类型化消息。基于 `int eventId` 的旧路径更适合兼容场景，而不是新接口设计。

### 日志系统

日志入口是静态类 `Logger`。分类日志可通过 `Logger.GetLogger(string)` 或 `Logger.GetLogger<T>()` 获取。

```csharp
using vFrame.Core;

Logger.ApplyConfiguration(new LogConfiguration {
    GlobalMinimumLevel = LogLevelDef.Info,
    FileLogPath = "Logs/core.log",
    CaptureStackTrace = true
});

var logger = Logger.GetLogger("Bootstrap");
logger.Info("system ready");

Logger.Warning("fallback warning");
Logger.Error(new System.Exception("boom"), "startup failed");
```

### 压缩与加密

常规流压缩入口是 `CompressorPool`：

```csharp
using System.IO;
using vFrame.Core;

var compressor = CompressorPool.Instance().Rent(CompressorType.LZ4);
using var input = new MemoryStream(new byte[] { 1, 2, 3, 4 });
using var output = new MemoryStream();
compressor.Compress(input, output);
compressor.Destroy();
```

分块压缩入口是 `SynchronousBlockBasedCompression`：

```csharp
using System.IO;
using vFrame.Core;

var compression = new SynchronousBlockBasedCompression();
compression.Create();
compression.Compress(
    new MemoryStream(new byte[] { 1, 2, 3, 4 }),
    new MemoryStream(),
    new BlockBasedCompressionOptions {
        CompressorType = CompressorType.Zlib,
        BlockSize = 1024
    },
    null);
compression.Destroy();
```

加密入口是 `EncryptorPool`：

```csharp
using vFrame.Core;

var encryptor = EncryptorPool.Instance().Rent(EncryptorType.Xor);
var key = new byte[] { 1, 2, 3, 4 };
var input = new byte[] { 10, 20, 30, 40 };
var output = new byte[input.Length];

encryptor.Encrypt(input, output, key, key.Length);
encryptor.Destroy();
```

### Unity 扩展 API

#### `SpawnPools`

`SpawnPools` 负责 Unity 侧的实例复用，关键公开入口包括：

- `Create(IGameObjectLoaderFactory, SpawnPoolsSettings)`
- `Spawn(string assetPath, Transform parent = null)`
- `SpawnAsync(string assetPath, Transform parent = null)`
- `PreloadAsync(string[] assetPaths)`
- `Recycle(GameObject obj)`
- `Update()`
- `Clear()`

```csharp
using UnityEngine;
using vFrame.Core.Unity;

var pools = new SpawnPools();
pools.Create(null, SpawnPoolsSettings.Default);

var player = pools.Spawn("Assets/Prefabs/Player.prefab");
pools.Recycle(player);
pools.Update();
pools.Destroy();
```

#### `CoroutinePool`

`CoroutinePool` 通过构造函数直接创建，不继承 `BaseObject`：

```csharp
using System.Collections;
using vFrame.Core.Unity;

var coroutinePool = new CoroutinePool("Gameplay", capacity: 8);
var handle = coroutinePool.StartCoroutine(Run());
coroutinePool.PauseCoroutine(handle);
coroutinePool.UnPauseCoroutine(handle);
coroutinePool.StopCoroutine(handle);
coroutinePool.Destroy();

IEnumerator Run()
{
    yield return null;
}
```

#### `Downloader`

`Downloader` 是 `MonoBehaviour` 组件，可通过 `Downloader.Create()` 快速生成：

```csharp
using vFrame.Core.Unity;

var downloader = Downloader.Create();
downloader.Timeout = 120;
downloader.AddDownload("D:/Temp/config.json", "https://example.com/config.json");
```

## 架构概览

### 程序集结构

当前子仓库的主要 asmdef：

- `Assets/vFrame.Core/Runtime/vFrame.Core.asmdef`
- `Assets/vFrame.Core.Unity/Runtime/vFrame.Core.Unity.asmdef`
- `Assets/vFrame.Core.Tests/EditMode/vFrame.Core.Tests.EditMode.asmdef`
- `Assets/vFrame.Core.Tests/EditMode/Asynchronous/vFrame.Core.Tests.EditMode.Unity.asmdef`
- `Assets/vFrame.Core.Tests/PlayMode/vFrame.Core.Tests.PlayMode.asmdef`

依赖方向如下：

- `vFrame.Core`：无程序集依赖
- `vFrame.Core.Unity`：依赖 `vFrame.Core`
- `vFrame.Core.Tests.EditMode`：依赖 `vFrame.Core`
- `vFrame.Core.Tests.EditMode.Unity`：依赖 `vFrame.Core` 与 `vFrame.Core.Unity`
- `vFrame.Core.Tests.PlayMode`：依赖 `vFrame.Core` 与 `vFrame.Core.Unity`

### 目录分层

```text
Assets/
├── vFrame.Core/
│   ├── Runtime/
│   │   ├── Async/
│   │   ├── Base/
│   │   ├── Compression/
│   │   ├── Dispatchers/
│   │   ├── Encryption/
│   │   ├── Exceptions/
│   │   ├── Extensions/
│   │   ├── Generic/
│   │   ├── Loggers/
│   │   ├── ObjectPools/
│   │   └── Utils/
│   ├── Editor/
│   └── package.json
├── vFrame.Core.Unity/
│   ├── Runtime/
│   │   ├── Asynchronous/
│   │   ├── Coroutine/
│   │   ├── Download/
│   │   ├── Extensions/
│   │   ├── Generic/
│   │   ├── Loggers/
│   │   ├── Patch/
│   │   ├── SpawnPools/
│   │   └── Utils/
│   ├── Editor/
│   └── package.json
└── vFrame.Core.Tests/
    ├── EditMode/
    └── PlayMode/
```

## 注意事项

### `System` 类型冲突

`vFrame.Core` 自身定义了一些可能与 `System` 命名空间冲突的类型。出现歧义时请显式加前缀，例如：

```csharp
System.ArgumentNullException
System.InvalidOperationException
```

### `ILogger` 歧义

如果同时引用 `UnityEngine` 与 `vFrame.Core`，`ILogger` 可能冲突，建议显式使用别名：

```csharp
using vFrameCore = vFrame.Core;
```

随后写成 `vFrameCore.ILogger`。

### 生命周期语义

本库推荐显式区分三种资源关系：

- `owned`
- `borrowed`
- `lifetime-bound`

这有助于在 `BaseObject` 与 `ILifetime` 的边界内明确谁负责清理资源。

### `BaseObject` 是终态生命周期

`Destroy()` 之后对象不能再次 `Create()`。如果需要复用对象，请使用对象池，而不是重新激活同一个 `BaseObject` 实例。

### `Dispatcher` 需要显式 `Create()` / `Destroy()`

`Dispatcher` 继承自 `BaseObject`，使用前必须先 `Create()`，结束后应调用 `Destroy()`：

```csharp
var dispatcher = new Dispatcher();
dispatcher.Create();
// ...
dispatcher.Destroy();
```

### 退役模块

仓库中仍保留一些历史模块用于兼容，但不建议在新功能中继续扩展：

- `Container/Component`
- `Profiles`
- `Localization`
- `MultiThreading/Task`
- `Patch`
- `Download`

## 许可证

本项目基于 [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0) 许可协议发布。
