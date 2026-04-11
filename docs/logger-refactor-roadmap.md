# vFrame.Core Logger 渐进式自研改造路线图

> 版本: v1.0 | 日期: 2026-04-11 | 作者: Hermes Agent

---

## 前置分析 — 现有调用点审计

全项目共 **59 处** Logger 调用，分布如下：

### 活跃模块（19 处）

| 文件 | 调用数 | 形式 | 说明 |
|---|---|---|---|
| Dispatcher.cs | 10 | 有 Tag | event/command/request/decision 分发 |
| SpawnPoolsDebug.cs | 3 | 有 Tag | 对象池诊断门面 |
| CoroutinePoolDebug.cs | 3 | 有 Tag | 含 1 个 bug：Error 方法误调 Warning |
| PerfProfile.cs | 1 | 有 Tag | 性能计时输出 |
| PoolObjectIdentity.cs | 1 | 无 Tag | 对象被外部销毁告警 |
| PreloadAsyncRequest.cs | 2 | 无 Tag | Tag 硬编码在字符串 "[SpawnPools]" |

### Retiring 模块（40 处）

| 文件 | 调用数 | 形式 |
|---|---|---|
| Patcher.cs | 20 | 有 Tag |
| HashChecker.cs | 5 | 有 Tag |
| Manifest.cs | 4 | 有 Tag（含 1 个 bug：加载失败用 Info） |
| Localization.cs | 3 | 有 Tag |
| DownloadAgentUnityWebRequest.cs | 4 | 无 Tag |
| ThreadedTask.cs | 2 | 无 Tag |
| ParallelTaskRunner.cs | 1 | 无 Tag |
| Container.cs | 1 | 无 Tag |

### 关键发现

- **有 Tag 调用占 64%**（50/59），全部使用 `static readonly LogTag` 字段
- **无 Tag 调用集中在 retiring 模块**（8/9 处）
- **零使用的高级 API**：`skip` 参数重载、`Fatal(Exception)`、`AddSink`/`RemoveSink`
- **唯一外部集成点**：`UnityLogger.cs`（通过 `OnLogReceived` 事件桥接）
- **已发现 2 个 bug**：
  1. `CoroutinePoolDebug.cs:44` — `Logger.Warning` 应为 `Logger.Error`
  2. `Manifest.cs:411` — `Logger.Info` 应为 `Logger.Error`

---

## Phase 0 — 兼容性基座

**目标**：在不改动 Logger 内部实现的前提下，为后续所有 Phase 建立安全的迁移基座。

### 具体工作

**0.1 修复已发现的调用 bug**
- `CoroutinePoolDebug.cs:44` — `Logger.Warning` → `Logger.Error`
- `Manifest.cs:411` — `Logger.Info` → `Logger.Error`

**0.2 为 retiring 模块标记日志调用**
- 在 retiring 模块的 Logger 调用处添加 `[Obsolete]` 或条件编译标记
- 这些调用在后续 Phase 中不需要迁移

**0.3 补充测试基线**
- 为现有 Logger 行为编写特征测试（characterization tests）
- 覆盖：格式化输出格式、级别过滤逻辑、缓冲区行为、LogToFile 基本功能

### 涉及文件

| 文件 | 变更类型 |
|---|---|
| CoroutinePoolDebug.cs | bug 修复 |
| Manifest.cs | bug 修复 |
| 新增 `Tests/EditMode/Loggers/LoggerBaselineTests.cs` | 新增测试 |

### 向后兼容：✅ 100% 兼容，纯增量改动

### 预估工作量：0.5 天

---

## Phase 1 — LogLevelDef 修正 + Trace 级别

**目标**：修正日志级别的语义设计，消除位掩码与序数比较的矛盾。

### 具体工作

**1.1 重写 LogLevelDef**

```csharp
// Before — 位掩码值，但用 > 做序数比较（语义矛盾）
public enum LogLevelDef {
    Debug = 1, Info = 2, Warning = 4, Error = 8, Fatal = 16
}

// After — 连续序数，> 比较语义正确
public enum LogLevelDef {
    Trace = 0, Debug = 1, Info = 2, Warning = 3, Error = 4, Fatal = 5
}
```

**1.2 调整过滤逻辑**

过滤逻辑 `if (LogLevel > level) return;` 值相同不变，逻辑一致：
- 设置 `LogLevel=Debug` 时，`Trace` 被过滤 ✓
- 设置 `LogLevel=Warning` 时，`Debug`/`Info` 被过滤 ✓

**1.3 添加 Trace 级别方法**

```csharp
public static void Trace(LogTag tag, string text, params object[] args)
public static void Trace(string text, params object[] args)
```

**1.4 在 LogFormatType 中增加可选字段**

```csharp
public const int Thread = 1 << 4;  // 线程 ID
public const int Line   = 1 << 5;  // 行号
```

### 涉及文件

| 文件 | 变更类型 |
|---|---|
| LogLevelDef.cs | 重写枚举值 |
| Logger.cs | 添加 Trace 方法、调整默认级别 |
| LogFormatType.cs | 添加 Thread/Line 常量 |

### 向后兼容：⚠️ 枚举值变化是 breaking change

- 内部调用都用枚举名而非数值，风险极低
- 外部用户如果用数值比较 `(int)LogLevelDef.Debug` 会受影响
- Logger 是 static 类无序列化场景

### 预估工作量：0.5 天

---

## Phase 2 — 调用点信息捕获改造

**目标**：消除每条日志的全堆栈反射开销，这是当前最大的性能瓶颈。

### 具体工作

**2.1 用 CallerMemberName / CallerFilePath / CallerLineNumber 替代 StackFrame**

```csharp
// Before — 每条日志 2 次堆栈遍历 + N 次反射
private static void Log(int skip, LogLevelDef level, LogTag tag, string text, params object[] args) {
    var content = GetFormattedLogText(skip, tag, logText);  // new StackFrame + 反射
    var stack = GetLogStack(skip);  // new StackTrace(1, true) + 完整遍历
}

// After — 编译器注入，零运行时开销
public static void Info(LogTag tag, string text, params object[] args,
    [CallerMemberName] string member = "",
    [CallerFilePath] string file = "",
    [CallerLineNumber] int line = 0) {
    Log(LogLevelDef.Info, tag, text, args, member, file, line);
}
```

**2.2 LogContext 增加结构化调用点字段**

```csharp
public struct LogContext {
    public LogLevelDef Level;
    public LogTag Tag;
    public string Content;           // 已格式化文本（向后兼容）
    public string MessageTemplate;   // 新增：原始模板
    public object[] Args;            // 新增：原始参数
    public Exception Exception;      // 扩展：所有级别都支持
    // 新增结构化调用点
    public string MemberName;
    public string FilePath;
    public int LineNumber;
    public string StackTrace;        // 仅 Error/Fatal 或显式请求时填充
}
```

**2.3 StackTrace 改为按需捕获**

- 普通日志（Debug/Info/Warning）：`StackTrace = null`，零开销
- Error/Fatal：仅当有 Exception 时自动捕获
- 提供 `Logger.CaptureStackTrace = true` 开关强制全部捕获（默认关闭）

### 涉及文件

| 文件 | 变更类型 |
|---|---|
| Logger.cs | 重写公开方法签名、LogContext 结构体、格式化逻辑 |

### 向后兼容

- `Logger.Debug(tag, text, args)` 签名不变（params 后面加可选参数，C# 兼容）
- `LogContext.Content` 字段保留，UnityLogger 和 LogToFile 不受影响
- `skip` 参数重载将被废弃（零使用，可安全移除）
- `LogContext.StackTrace` 在默认配置下变为 null

### 性能收益

- 消除每条日志的 2 次 StackTrace/StackFrame 创建
- 消除 N 次反射调用（N=堆栈深度，通常 20-50）
- 消除 1-5KB 的堆栈字符串分配
- **预估热路径性能提升 10-50 倍**

### 预估工作量：2 天（⚠️ 最大风险点）

---

## Phase 3 — LogToFile 紧急修复

**目标**：解决生产环境数据丢失风险。

### 具体工作

**3.1 异常不再静默吞掉**

```csharp
// Before
catch (Exception) { }

// After
catch (OperationCanceledException) { break; }
catch (Exception ex) {
    System.Diagnostics.Debug.WriteLine($"[LogToFile] Write failed: {ex.Message}");
}
```

**3.2 紧急 Flush 机制**

```csharp
public void AppendText(string value, bool urgent = false) {
    // ...
    if (urgent) {
        WriteAllText();  // Error/Fatal 立即刷盘
    }
}
```

**3.3 文件句柄保持打开**

```csharp
// Before：每次 WriteAllText 都 File.OpenWrite + Seek + Dispose
// After：OnCreate 时打开，OnDestroy 时关闭

private FileStream _fileStream;
private StreamWriter _writer;

protected override void OnCreate(string path) {
    // ...
    _fileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
    _writer = new StreamWriter(_fileStream) { AutoFlush = false };
}
```

**3.4 OnDestroy 时保证最终刷写**

```csharp
protected override void OnDestroy() {
    _cancellationTokenSource.Cancel();
    _task?.Wait(5000);  // 最多等 5 秒
    WriteAllText();      // 确保缓冲区清空
    _writer?.Dispose();
    _fileStream?.Dispose();
}
```

### 涉及文件

| 文件 | 变更类型 |
|---|---|
| LogToFile.cs | 重写核心写入逻辑 |
| Logger.cs | AppendText 传递 urgent 参数 |

### 向后兼容：✅ 100% 兼容，纯内部实现变更

### 预估工作量：1 天

---

## Phase 4 — 异常日志全级别支持

**目标**：Error/Warning/Info/Debug 级别都能直接传 Exception。

### 具体工作

**4.1 添加全级别异常重载**

```csharp
public static void Debug(LogTag tag, Exception exception, string text = null)
public static void Info(LogTag tag, Exception exception, string text = null)
public static void Warning(LogTag tag, Exception exception, string text = null)
public static void Error(LogTag tag, Exception exception, string text = null)
public static void Fatal(LogTag tag, Exception exception, string text = null)
```

**4.2 UnityLogger 增强异常处理**

```csharp
private static void OnLogReceived(Logger.LogContext context) {
    if (context.Exception != null) {
        Debug.LogException(context.Exception);
        return;
    }
    // ... 现有级别分支
}
```

### 涉及文件

| 文件 | 变更类型 |
|---|---|
| Logger.cs | 添加方法重载 |
| UnityLogger.cs | 增强异常处理逻辑 |

### 向后兼容：✅ 100% 兼容，纯增量

### 预估工作量：0.5 天

---

## Phase 5 — 结构化 LogContext + ILogSink 升级

**目标**：让 Sink 能拿到原始参数，为结构化输出铺路。

### 具体工作

**5.1 添加 IStructuredLogSink 接口**

```csharp
// 旧 ILogSink 保持兼容，内部仍然接收格式化后的 Content
// 新增结构化接口，LogContext 已在 Phase 2 增加了 MessageTemplate + Args
public interface IStructuredLogSink {
    void OnLogReceived(LogContext context);
}
```

**5.2 Logger 内部同时维护两份 sink 列表**

```csharp
private static void EmitToSinks(LogContext context) {
    // 结构化 sink 拿到完整 LogContext（含原始参数）
    foreach (var sink in _structuredSinks) {
        sink.OnLogReceived(context);
    }
    // 旧 sink 拿到的 LogContext.Content 仍是格式化文本（兼容）
    foreach (var sink in _legacySinks) {
        sink.OnLogReceived(context);
    }
}
```

**5.3 添加示例结构化 Sink：JsonLogSink**

```json
{
  "level": "Error",
  "tag": "Dispatcher",
  "member": "Publish",
  "line": 127,
  "template": "Exception occurred, event type: {0}",
  "args": ["vFrame.Core.SomeEvent"],
  "time": "2024-01-15T12:34:56.789Z"
}
```

### 涉及文件

| 文件 | 变更类型 |
|---|---|
| Logger.cs | IStructuredLogSink、双 sink 列表、JsonLogSink |
| 新增 `Runtime/Loggers/JsonLogSink.cs` | 新增文件 |

### 向后兼容：✅ 旧 ILogSink 完全不受影响

### 预估工作量：1 天

---

## Phase 6 — Per-Sink 级别过滤

**目标**：不同输出目标可以设置不同的最低级别。

### 具体工作

**6.1 AddSink 时接受可选的 minLevel 参数**

```csharp
// 推荐：不改 ILogSink 接口，在注册时指定
public static void AddSink(ILogSink sink, LogLevelDef minLevel = LogLevelDef.Trace) {
    // 内部包装为 (sink, minLevel) 元组
}
```

**6.2 EmitToSinks 添加 per-sink 过滤**

```csharp
private static void EmitToSinks(LogContext context) {
    foreach (var (sink, minLevel) in _sinks) {
        if (context.Level >= minLevel) {
            sink.OnLogReceived(context);
        }
    }
}
```

### 使用示例

```csharp
Logger.AddSink(consoleSink, LogLevelDef.Debug);   // 控制台看全部
Logger.AddSink(fileSink, LogLevelDef.Warning);     // 文件只写 Warning+
Logger.AddSink(remoteSink, LogLevelDef.Error);     // 远程只发 Error+
```

### 涉及文件

| 文件 | 变更类型 |
|---|---|
| Logger.cs | AddSink 签名变更、EmitToSinks 过滤 |

### 向后兼容：✅ minLevel 有默认值，旧调用无需改动

### 预估工作量：0.5 天

---

## Phase 7 — Per-Module Logger

**目标**：不同模块可以设置不同的日志级别。

### 具体工作

**7.1 引入 ILogger 接口**

```csharp
public interface ILogger {
    void Trace(string text, params object[] args);
    void Debug(string text, params object[] args);
    void Info(string text, params object[] args);
    void Warning(string text, params object[] args);
    void Error(string text, params object[] args);
    void Fatal(string text, params object[] args);
    void Error(Exception exception, string text = null);
    void Fatal(Exception exception, string text = null);
    string CategoryName { get; }
    LogLevelDef MinimumLevel { get; set; }
}
```

**7.2 实现类 LoggerCategory**

```csharp
public sealed class LoggerCategory : ILogger {
    public LogLevelDef MinimumLevel { get; set; } = LogLevelDef.Trace;

    public void Info(string text, params object[] args) {
        if (MinimumLevel > LogLevelDef.Info) return;
        Logger.Info(_tag, text, args);
    }
    // ...
}
```

**7.3 Logger 添加工厂方法**

```csharp
// 按类型获取
public static ILogger GetLogger<T>() => GetLogger(typeof(T).FullName);

// 按名称获取
public static ILogger GetLogger(string categoryName) { ... }

// 按命名空间规则设置级别
public static void SetLevel(string categoryPrefix, LogLevelDef level) { ... }
```

**7.4 活跃模块迁移**

```csharp
// Before
private static readonly LogTag LogTag = new LogTag("Dispatcher");
Logger.Error(LogTag, "Exception occurred, event type: {0}", type.FullName);

// After
private static readonly ILogger Log = Logger.GetLogger<Dispatcher>();
Log.Error("Exception occurred, event type: {0}", type.FullName);

// 按命名空间控制级别
Logger.SetLevel("vFrame.Core.Dispatchers", LogLevelDef.Debug);
Logger.SetLevel("vFrame.Core.SpawnPools", LogLevelDef.Warning);
```

### 涉及文件

| 文件 | 变更类型 |
|---|---|
| 新增 `Runtime/Loggers/ILogger.cs` | 接口定义 |
| 新增 `Runtime/Loggers/LoggerCategory.cs` | 实现类 |
| Logger.cs | 添加 GetLogger/SetLevel 工厂方法 |
| 活跃模块文件 | 自愿迁移（渐进式） |

### 向后兼容：✅ 所有现有 static 方法保留不动，retiring 模块不迁移

### 预估工作量：1.5 天

---

## Phase 8 — 日志上下文（Scope / Enricher）

**目标**：支持请求级别的上下文传播（RequestId、PlayerId 等）。

### 具体工作

**8.1 LogContext 增加属性字典**

```csharp
public struct LogContext {
    // ... 现有字段
    public IReadOnlyDictionary<string, object> Properties;  // 新增
}
```

**8.2 实现 LogScope**

```csharp
public readonly struct LogScope : IDisposable {
    public LogScope(string key, object value) {
        LogContext.PushProperty(key, value);
    }
    public void Dispose() => LogContext.PopProperty(_key);
}

// 内部使用 AsyncLocal<T> 实现上下文传播
```

**8.3 使用方式**

```csharp
using (new LogScope("RequestId", requestId))
using (new LogScope("PlayerId", playerId)) {
    Log.Info("Processing order {OrderId}", orderId);
    // LogContext.Properties 自动包含 RequestId + PlayerId
}
```

### 涉及文件

| 文件 | 变更类型 |
|---|---|
| 新增 `Runtime/Loggers/LogScope.cs` | Scope 实现 |
| 新增 `Runtime/Loggers/LogContextProperties.cs` | 上下文属性存储 |
| Logger.cs | LogContext 结构体增加 Properties |

### 向后兼容：✅ Properties 默认为空字典

### 预估工作量：1.5 天

---

## Phase 9 — LogToFile 日志滚动

**目标**：解决单文件无限增长问题。

### 具体工作

**9.1 滚动策略**

```csharp
public enum RollingStrategy {
    None,
    BySize,      // 按文件大小
    ByDate       // 按日期
}

public class LogToFileOptions {
    public string FilePath { get; set; }
    public RollingStrategy Strategy { get; set; } = RollingStrategy.ByDate;
    public long MaxFileSizeBytes { get; set; } = 50 * 1024 * 1024;  // 50MB
    public int MaxFileCount { get; set; } = 7;   // 保留 7 个文件
    public bool CompressArchives { get; set; } = false;
    public int FlushIntervalSeconds { get; set; } = 1;  // 从 10 秒降到 1 秒
}
```

**9.2 按日期滚动**

- 文件名：`log_2024-01-15.txt`
- 每天零点自动切换到新文件
- 超过 MaxFileCount 的旧文件自动删除（或压缩归档）

**9.3 按大小滚动**

- 当前文件超过 MaxFileSizeBytes 时重命名归档
- `log.txt` → `log_20240115_123456.txt`
- 创建新的 `log.txt` 继续写入

**9.4 Logger.LogFilePath 升级**

```csharp
// 旧 API 保持兼容，内部默认使用按日期滚动
Logger.LogFilePath = "/path/to/log.txt";

// 新 API：完整配置
Logger.ConfigureFileLogging(new LogToFileOptions {
    FilePath = "/path/to/log.txt",
    Strategy = RollingStrategy.BySize,
    MaxFileSizeBytes = 100 * 1024 * 1024,
    MaxFileCount = 30
});
```

### 涉及文件

| 文件 | 变更类型 |
|---|---|
| LogToFile.cs | 大幅重写，添加滚动逻辑 |
| 新增 `Runtime/Loggers/RollingStrategy.cs` | 枚举 |
| 新增 `Runtime/Loggers/LogToFileOptions.cs` | 配置类 |
| Logger.cs | 添加 ConfigureFileLogging 方法 |

### 向后兼容：✅ Logger.LogFilePath 保持可用

### 预估工作量：2 天

---

## Phase 10 — 格式化模板系统

**目标**：替代硬编码的 4 位开关，支持自定义输出格式。

### 具体工作

**10.1 模板语法**

```
{time}                          → [12:34:56:789]（默认格式）
{time:yyyy-MM-dd HH:mm:ss}      → 自定义格式
{level}                         → INFO / WARNING / ERROR
{level:u3}                      → INF / WRN / ERR（短格式）
{tag}                           → LogTag 名称
{class}                         → 类名
{method}                        → 方法名
{line}                          → 行号
{thread}                        → 线程 ID
{message}                       → 日志消息
{exception}                     → 异常信息
{property:Key}                  → 上下文属性
```

**10.2 LogFormatter 实现**

```csharp
public class LogFormatter {
    public LogFormatter(string template) { _tokens = Parse(template); }
    public string Format(LogContext context) { ... }
}
```

**10.3 预置模板**

```csharp
public static class LogTemplates {
    public const string Default  = "{tag} {time} {class}::{method} {message}";
    public const string Compact  = "{time:HH:mm:ss} {level:u3} {message}";
    public const string Verbose  = "{time:yyyy-MM-dd HH:mm:ss.fff} [{level:u3}] [{thread}] {class}::{method} {message}{newline}{exception}";
}
```

**10.4 替代 LogFormatMask**

```csharp
// Before
Logger.LogFormatMask = LogFormatType.Tag | LogFormatType.Time | LogFormatType.Class;

// After
Logger.LogFormatTemplate = LogTemplates.Default;
// LogFormatMask 标记 [Obsolete]，内部转换为等效模板
```

### 涉及文件

| 文件 | 变更类型 |
|---|---|
| 新增 `Runtime/Loggers/LogFormatter.cs` | 模板解析与渲染 |
| 新增 `Runtime/Loggers/Tokens/*.cs` | 各 Token 实现 |
| 新增 `Runtime/Loggers/LogTemplates.cs` | 预置模板 |
| Logger.cs | 添加 LogFormatTemplate，LogFormatMask 标记 Obsolete |

### 向后兼容：✅ LogFormatMask 标记 [Obsolete] 但保持可用

### 预估工作量：2 天

---

## Phase 11 — 运行时配置

**目标**：支持不重启地动态调整日志级别和格式。

### 具体工作

**11.1 配置模型**

```csharp
public class LogConfiguration {
    public LogLevelDef GlobalMinimumLevel { get; set; } = LogLevelDef.Warning;
    public Dictionary<string, LogLevelDef> CategoryLevels { get; set; }
    public string FileLogPath { get; set; }
    public LogToFileOptions FileLogOptions { get; set; }
    public string FormatTemplate { get; set; }
    public bool CaptureStackTrace { get; set; }
}
```

**11.2 运行时重载**

```csharp
public static class Logger {
    public static void ApplyConfiguration(LogConfiguration config) {
        LogLevel = config.GlobalMinimumLevel;
        LogFormatTemplate = config.FormatTemplate;
        CaptureStackTrace = config.CaptureStackTrace;
        foreach (var kv in config.CategoryLevels) {
            SetLevel(kv.Key, kv.Value);
        }
    }

    public static void LoadConfiguration(string jsonPath) {
        var json = File.ReadAllText(jsonPath);
        var config = JsonConvert.DeserializeObject<LogConfiguration>(json);
        ApplyConfiguration(config);
    }
}
```

**11.3 可选：文件监听热重载**

```csharp
// 使用 FileSystemWatcher 监听配置文件变化
// 变化时自动 LoadConfiguration + ApplyConfiguration
```

### 配置文件示例（JSON）

```json
{
  "globalMinimumLevel": "Warning",
  "categoryLevels": {
    "vFrame.Core.Dispatchers": "Debug",
    "vFrame.Core.SpawnPools": "Info",
    "vFrame.Core.Network": "Trace"
  },
  "fileLogPath": "/var/log/mygame/game.log",
  "fileLogOptions": {
    "strategy": "ByDate",
    "maxFileCount": 14,
    "flushIntervalSeconds": 1
  },
  "formatTemplate": "{time:HH:mm:ss} {level:u3} [{tag}] {message}",
  "captureStackTrace": false
}
```

### 涉及文件

| 文件 | 变更类型 |
|---|---|
| 新增 `Runtime/Loggers/LogConfiguration.cs` | 配置模型 |
| Logger.cs | 添加 ApplyConfiguration/LoadConfiguration |

### 向后兼容：✅ 100% 兼容，纯增量

### 预估工作量：1 天

---

## 实施优先级与依赖关系

```
Phase 0 (兼容基座)
  ├→ Phase 1 (LogLevelDef 修正)     ← 无依赖
  ├→ Phase 2 (调用点改造)           ← 无依赖  ⚠️ 性能收益最大
  ├→ Phase 3 (LogToFile 紧急修复)   ← 无依赖  ⚠️ 数据安全最急
  └→ Phase 4 (异常全级别)           ← 依赖 Phase 2

Phase 1+2 完成后 → Phase 5 (结构化 Sink) → Phase 6 (Per-Sink 过滤)
Phase 5 完成后   → Phase 7 (Per-Module Logger)
Phase 7 完成后   → Phase 8 (Scope/Enricher)
Phase 3 完成后   → Phase 9 (日志滚动)
Phase 2 完成后   → Phase 10 (格式化模板)
Phase 10+11 可并行
```

### 推荐实施顺序

```
Phase 3 → Phase 1 → Phase 2 → Phase 4 → Phase 6 → Phase 10
→ Phase 9 → Phase 5 → Phase 7 → Phase 8 → Phase 11
```

**理由**：
- Phase 3 最急（数据安全），Phase 1 最简单（热身），Phase 2 收益最大
- Phase 4 依赖 Phase 2 的 LogContext 改造
- Phase 6/10 是独立增强，Phase 9 依赖 Phase 3
- Phase 5/7/8 是高级特性，放后期

---

## 工作量汇总

| Phase | 名称 | 预估天数 | 风险等级 |
|---|---|---|---|
| 0 | 兼容性基座 | 0.5 | 低 |
| 1 | LogLevelDef 修正 | 0.5 | 低 |
| 2 | 调用点改造 | 2 | ⚠️ 中 |
| 3 | LogToFile 紧急修复 | 1 | 低 |
| 4 | 异常全级别 | 0.5 | 低 |
| 5 | 结构化 Sink | 1 | 低 |
| 6 | Per-Sink 过滤 | 0.5 | 低 |
| 7 | Per-Module Logger | 1.5 | 低 |
| 8 | Scope/Enricher | 1.5 | 低 |
| 9 | 日志滚动 | 2 | 低 |
| 10 | 格式化模板 | 2 | 低 |
| 11 | 运行时配置 | 1 | 低 |
| **合计** | | **14 天** | |

---

## 改造后 vs 主流库差距对比

| 维度 | 改造前 | 改造后（全部完成） | Serilog/NLog |
|---|---|---|---|
| 结构化日志 | ❌ 无 | ✅ MessageTemplate + Args | ✅ |
| 上下文/Scope | ❌ 无 | ✅ LogScope + AsyncLocal | ✅ |
| Per-module 级别 | ❌ 无 | ✅ GetLogger + SetLevel | ✅ |
| Per-sink 级别 | ❌ 无 | ✅ AddSink minLevel | ✅ |
| 格式模板 | 4 位开关 | ✅ 自定义 {time} {level} 等 | 完整模板语言 |
| 调用点开销 | 全堆栈反射 | ✅ CallerMemberName (0ns) | 可选 StackFrame |
| 字符串分配 | 3-5次+堆栈 | ✅ 1-2次（无堆栈） | 2-3次 |
| 文件滚动 | ❌ 无 | ✅ 按天/按大小 | 按天/大小/时间 |
| 紧急 Flush | ❌ 无 | ✅ Error/Fatal 立即 | 可配置 |
| 异常日志 | 仅 Fatal | ✅ 全级别 | 全级别 |
| Enricher | ❌ 无 | ✅ LogScope 属性 | 完整体系 |
| 运行时配置 | ❌ 无 | ✅ JSON 热重载 | XML/JSON/Code |
| 采样限流 | ❌ 无 | ❌ 无（后续可补） | ✅ |

### 改造后剩余差距（可接受）

- 模板语言不如 NLog 丰富（无 100+ 内置 renderer，够用即可）
- 无原生 OpenTelemetry 集成（通过 JSON Sink 可间接对接 ELK/Seq）
- 无日志采样（高频场景需手动控制，或后续补充 Phase 12）
